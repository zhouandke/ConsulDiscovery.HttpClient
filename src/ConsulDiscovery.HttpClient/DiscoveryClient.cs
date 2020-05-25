using Consul;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ConsulDiscovery.HttpClient
{
    public class DiscoveryClient : IDisposable
    {
        private readonly ConsulDiscoveryOptions consulDiscoveryOptions;
        private readonly Timer timer;
        private readonly ConsulClient consulClient;
        private readonly string serviceIdInConsul;

        public Dictionary<string, List<string>> AllServices { get; private set; } = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);


        public DiscoveryClient(IOptions<ConsulDiscoveryOptions> options)
        {
            consulDiscoveryOptions = options.Value;
            consulClient = new ConsulClient(x => x.Address = new Uri($"http://{consulDiscoveryOptions.ConsulServerSetting.IP}:{consulDiscoveryOptions.ConsulServerSetting.Port}"));
            timer = new Timer(Refresh);

            if (consulDiscoveryOptions.ServiceRegisterSetting != null)
            {
                serviceIdInConsul = Guid.NewGuid().ToString();
            }
        }

        public void Start()
        {
            var checkErrorMsg = CheckParams();
            if (checkErrorMsg != null)
            {
                throw new ArgumentException(checkErrorMsg);
            }
            RegisterToConsul();
            timer.Change(0, consulDiscoveryOptions.ConsulServerSetting.RefreshIntervalInMilliseconds);
        }

        public void Stop()
        {
            Dispose();
        }

        private string CheckParams()
        {
            if (string.IsNullOrWhiteSpace(consulDiscoveryOptions.ConsulServerSetting.IP))
            {
                return "Consul服务器地址 ConsulDiscoveryOptions.ConsulServerSetting.IP 不能为空";
            }

            if (consulDiscoveryOptions.ServiceRegisterSetting != null)
            {
                var registerSetting = consulDiscoveryOptions.ServiceRegisterSetting;
                if (string.IsNullOrWhiteSpace(registerSetting.ServiceName))
                {
                    return "服务名称 ConsulDiscoveryOptions.ServiceRegisterSetting.ServiceName 不能为空";
                }
                if (string.IsNullOrWhiteSpace(registerSetting.ServiceIP))
                {
                    return "服务地址 ConsulDiscoveryOptions.ServiceRegisterSetting.ServiceIP 不能为空";
                }
            }
            return null;
        }

        private void RegisterToConsul()
        {
            if (string.IsNullOrEmpty(serviceIdInConsul))
            {
                return;
            }

            var registerSetting = consulDiscoveryOptions.ServiceRegisterSetting;
            var httpCheck = new AgentServiceCheck()
            {
                HTTP = $"{registerSetting.ServiceScheme}{Uri.SchemeDelimiter}{registerSetting.ServiceIP}:{registerSetting.ServicePort}/{registerSetting.HealthCheckRelativeUrl.TrimStart('/')}",
                Interval = TimeSpan.FromMilliseconds(registerSetting.HealthCheckIntervalInMilliseconds),
                Timeout = TimeSpan.FromMilliseconds(registerSetting.HealthCheckTimeOutInMilliseconds),
                DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(10),
            };
            var registration = new AgentServiceRegistration()
            {
                ID = serviceIdInConsul,
                Name = registerSetting.ServiceName,
                Address = registerSetting.ServiceIP,
                Port = registerSetting.ServicePort,
                Check = httpCheck,
                Meta = new Dictionary<string, string>() { ["scheme"] = registerSetting.ServiceScheme },
            };
            consulClient.Agent.ServiceRegister(registration).Wait();
        }

        private void DeregisterFromConsul()
        {
            if (string.IsNullOrEmpty(serviceIdInConsul))
            {
                return;
            }
            try
            {
                consulClient.Agent.ServiceDeregister(serviceIdInConsul).Wait();
            }
            catch
            { }
        }

        private void Refresh(object state)
        {
            Dictionary<string, AgentService>.ValueCollection serversInConsul;
            try
            {
                serversInConsul = consulClient.Agent.Services().Result.Response.Values;
            }
            catch // (Exception ex)
            {
                // 如果连接consul出错, 则不更新服务列表. 继续使用以前获取到的服务列表
                // 但是如果很长时间都不能连接consul, 服务列表里的一些实例已经不可用了, 还一直提供这样旧的列表也不合理, 所以要不要在这里实现 健康检查? 这样的话, 就得把检查地址变成不能设置的
                return;
            }

            // 1. 更新服务列表
            // 2. 如果这个程序提供了服务, 还要检测 服务Id 是否在服务列表里
            var tempServices = new Dictionary<string, HashSet<string>>();
            bool needReregisterToConsul = true;
            foreach (var service in serversInConsul)
            {
                var serviceName = service.Service;
                if (!service.Meta.TryGetValue("scheme", out var serviceScheme))
                {
                    serviceScheme = Uri.UriSchemeHttp;
                }
                var serviceHost = $"{serviceScheme}{Uri.SchemeDelimiter}{service.Address}:{service.Port}";
                if (!tempServices.TryGetValue(serviceName, out var serviceHosts))
                {
                    serviceHosts = new HashSet<string>();
                    tempServices[serviceName] = serviceHosts;
                }
                serviceHosts.Add(serviceHost);

                if (needReregisterToConsul && !string.IsNullOrEmpty(serviceIdInConsul) && serviceIdInConsul == service.ID)
                {
                    needReregisterToConsul = false;
                }
            }

            if (needReregisterToConsul)
            {
                RegisterToConsul();
            }

            var tempAllServices = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in tempServices)
            {
                tempAllServices[item.Key] = item.Value.ToList();
            }
            AllServices = tempAllServices;
        }


        public void Dispose()
        {
            DeregisterFromConsul();
            consulClient.Dispose();
            timer.Dispose();
        }
    }
}

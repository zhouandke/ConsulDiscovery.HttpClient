using System;

namespace ConsulDiscovery.HttpClient
{
    public class ConsulDiscoveryOptions
    {
        public ConsulServerSetting ConsulServerSetting { get; set; } = new ConsulServerSetting();

        public ServiceRegisterSetting ServiceRegisterSetting { get; set; }
    }

    public class ConsulServerSetting
    {
        public string IP { get; set; } = "127.0.0.1";

        public int Port { get; set; } = 8500;

        public int RefreshIntervalInMilliseconds { get; set; } = 1000;
    }

    public class ServiceRegisterSetting
    {
        private string serviceScheme = Uri.UriSchemeHttp;

        public string ServiceName { get; set; }

        public string ServiceIP { get; set; }

        public int ServicePort { get; set; }

        /// <summary>
        /// http 或者  https, 默认 http
        /// </summary>
        public string ServiceScheme
        {
            get { return serviceScheme; }
            set
            {
                if (Uri.UriSchemeHttps.Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    serviceScheme = Uri.UriSchemeHttps;
                }
                else
                {
                    serviceScheme = Uri.UriSchemeHttp;
                }
            }
        }

        public string HealthCheckRelativeUrl { get; set; } = "/HealthCheck";

        public int HealthCheckIntervalInMilliseconds { get; set; } = 500;

        public int HealthCheckTimeOutInMilliseconds { get; set; } = 2000;
    }
}

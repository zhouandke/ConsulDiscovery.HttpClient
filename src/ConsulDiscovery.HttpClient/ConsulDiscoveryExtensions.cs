using ConsulDiscovery.HttpClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ConsulDiscoveryExtensions
    {
        public static IServiceCollection AddConsulDiscovery(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMvcCore().AddApplicationPart(typeof(ConsulDiscoveryExtensions).Assembly);// 注册 /HealthCheck
            services.Configure<ConsulDiscoveryOptions>(configuration.GetSection("ConsulDiscoveryOptions"));
            services.AddSingleton<DiscoveryClient>();
            services.AddTransient<DiscoveryHttpMessageHandler>();
            return services;
        }

        public static void StartConsulDiscovery(this IApplicationBuilder app, IHostApplicationLifetime lifetime)
        {
            var discoveryClient = app.ApplicationServices.GetRequiredService<DiscoveryClient>();
            lifetime.ApplicationStarted.Register(() => discoveryClient.Start());
            lifetime.ApplicationStopping.Register(() => discoveryClient.Stop());
        }
    }
}



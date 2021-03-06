using ConsulDiscovery.HttpClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace WebApplication1
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // ע�� ConsulDiscovery �������
            services.AddConsulDiscovery(Configuration);
            // ���� SayHelloService ��HttpClient
            services.AddHttpClient("SayHelloService", c =>
                {
                    c.BaseAddress = new Uri("http://SayHelloService");
                })
                .AddHttpMessageHandler<DiscoveryHttpMessageHandler>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // ���� ConsulDiscovery
            app.StartConsulDiscovery(lifetime);
        }
    }
}

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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // ×¢²á ConsulDiscovery Ïà¹ØÅäÖÃ
            services.AddConsulDiscovery(Configuration);
            // ÅäÖÃ SayHelloService µÄHttpClient
            services.AddHttpClient("SayHelloService", c =>
                {
                    c.BaseAddress = new Uri("http://SayHelloService");
                })
                .AddHttpMessageHandler<DiscoveryHttpMessageHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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

            // Æô¶¯ ConsulDiscovery
            app.StartConsulDiscovery(lifetime);
        }
    }
}

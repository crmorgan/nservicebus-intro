using System;
using Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using NServiceBus.Logging;

namespace store_web
{
    public class Startup
    {
        private static readonly ILog Log = LogManager.GetLogger<Startup>();

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            AddEndpoint(services);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private void AddEndpoint(IServiceCollection services)
        {
            Log.Info("****************** Store website endpoint starting ******************");

            var connectionString = Environment.GetEnvironmentVariable("servicebus_connection_string");
            var endpointConfiguration = new EndpointConfigurationBuilder("store-web", connectionString)
                                        .AsSendOnly()
                                        .Build();

            // start the endpoint
            var endpointInstance = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();
            Log.Info("****************** Store website endpoint successfully started ******************");

            // register endpoint instance with the IoC framework
            services.AddSingleton<IMessageSession>(endpointInstance);
        }
    }
}

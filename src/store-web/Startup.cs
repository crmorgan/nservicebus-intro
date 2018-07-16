using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using NServiceBus.Features;

namespace store_web
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
            services.AddMvc();
			AddEndpont(services);
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

	    private void AddEndpont(IServiceCollection services)
	    {
		    Console.WriteLine("Starting Endpoint");

		    var endpointConfiguration = new EndpointConfiguration("store-web");
			endpointConfiguration.DisableFeature<TimeoutManager>();
		    endpointConfiguration.SendFailedMessagesTo("error");
		    endpointConfiguration.AuditProcessedMessagesTo("audit");
		    endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
			endpointConfiguration.EnableInstallers();
			endpointConfiguration.SendOnly();

		    var connectionString = Environment.GetEnvironmentVariable("servicebus_connection_string");
			var transportExtensions = endpointConfiguration.UseTransport<RabbitMQTransport>();
		    transportExtensions.UseConventionalRoutingTopology();
		    transportExtensions.ConnectionString(connectionString);

			var endpointInstance = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();

		    services.AddSingleton<IMessageSession>(endpointInstance);
	    }
	}
}

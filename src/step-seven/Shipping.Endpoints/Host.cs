using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Infrastructure;
using NServiceBus;
using NServiceBus.Logging;

namespace Shipping.Endpoints
{
    class Host
    {
        // TODO: optionally choose a custom logging library
        // https://docs.particular.net/nservicebus/logging/#custom-logging
        // LogManager.Use<TheLoggingFactory>();
        static readonly ILog log = LogManager.GetLogger<Host>();

        IEndpointInstance endpoint;

        // TODO: give the endpoint an appropriate name
        public string EndpointName => "shipping";

        public async Task Start()
        {
            try
            {
                var connectionString = Environment.GetEnvironmentVariable("servicebus_connection_string");
                var endpointConfiguration = new EndpointConfigurationBuilder(EndpointName, connectionString)
                                            .WithPersistence()
                                            .Build();

                // start the endpoint
                endpoint = await Endpoint.Start(endpointConfiguration);
                log.Info("****************** Shipping endpoint successfully started ******************");
            }
            catch (Exception ex)
            {
                FailFast("Failed to start.", ex);
            }
        }

        public async Task Stop()
        {
            try
            {
                // TODO: perform any futher shutdown operations before or after stopping the endpoint
                await endpoint?.Stop();
            }
            catch (Exception ex)
            {
                FailFast("Failed to stop correctly.", ex);
            }
        }

        async Task OnCriticalError(ICriticalErrorContext context)
        {
            // TODO: decide if stopping the endpoint and exiting the process is the best response to a critical error
            // https://docs.particular.net/nservicebus/hosting/critical-errors
            // and consider setting up service recovery
            // https://docs.particular.net/nservicebus/hosting/windows-service#installation-restart-recovery
            try
            {
                await context.Stop();
            }
            finally
            {
                FailFast($"Critical error, shutting down: {context.Error}", context.Exception);
            }
        }

        void FailFast(string message, Exception exception)
        {
            try
            {
                log.Fatal(message, exception);

                // TODO: when using an external logging framework it is important to flush any pending entries prior to calling FailFast
                // https://docs.particular.net/nservicebus/hosting/critical-errors#when-to-override-the-default-critical-error-action
            }
            finally
            {
                Environment.FailFast(message, exception);
            }
        }
    }
}

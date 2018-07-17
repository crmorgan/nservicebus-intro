using System;
using System.Threading.Tasks;
using Infrastructure;
using NServiceBus;
using NServiceBus.Logging;

namespace Shipping.Endpoints
{
    class Host
    {
        private static readonly ILog Log = LogManager.GetLogger<Host>();
        private IEndpointInstance _endpoint;

        public string EndpointName => "shipping";

        public async Task Start()
        {
            try
            {
				var connectionString = Environment.GetEnvironmentVariable("servicebus_connection_string");
	            var endpointConfiguration = EndpointConfigurationBuilder.Build(EndpointName, connectionString, OnCriticalError);

	            // start the endpoint
	            _endpoint = await Endpoint.Start(endpointConfiguration);
	            Log.Info("****************** Shipping endpoint successfully started ******************");
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
                await _endpoint?.Stop();
            }
            catch (Exception ex)
            {
                FailFast("Failed to stop correctly.", ex);
            }
        }

        async Task OnCriticalError(ICriticalErrorContext context)
        {
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
                Log.Fatal(message, exception);
            }
            finally
            {
                Environment.FailFast(message, exception);
            }
        }
    }
}

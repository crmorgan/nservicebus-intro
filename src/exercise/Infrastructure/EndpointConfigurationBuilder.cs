using System;
using NServiceBus;

namespace Infrastructure
{
    public class EndpointConfigurationBuilder
    {
        private readonly EndpointConfiguration _endpointConfiguration;
        private readonly TransportExtensions<RabbitMQTransport> _transportExtensions;

        public EndpointConfigurationBuilder(string endpointName, string transportConnectionString)
        {
            _endpointConfiguration = new EndpointConfiguration(endpointName);

            // setup general
            _endpointConfiguration.UsePersistence<LearningPersistence>();
            _endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
            _endpointConfiguration.EnableInstallers();

            // setup auditing
            _endpointConfiguration.SendFailedMessagesTo("error");
            _endpointConfiguration.AuditProcessedMessagesTo("audit");

            // setup transport
            _transportExtensions = _endpointConfiguration.UseTransport<RabbitMQTransport>();
            _transportExtensions.UseConventionalRoutingTopology();
            _transportExtensions.ConnectionString(transportConnectionString);
        }

        public EndpointConfigurationBuilder AsSendOnly()
        {
            _endpointConfiguration.SendOnly();
            return this;
        }

        public EndpointConfigurationBuilder RouteToEndpoint(Type messageType, string endpointName)
        {
            var routing = _transportExtensions.Routing();
            routing.RouteToEndpoint(messageType, endpointName);

            return this;
        }

        public EndpointConfiguration Build()
        {
            return _endpointConfiguration;
        }
    }
}

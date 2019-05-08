using System;
using System.Threading.Tasks;
using NServiceBus;

namespace Infrastructure
{
	public static class EndpointConfigurationBuilder
	{
		public static EndpointConfiguration Build(string endpointName,
			string transportConnectionString,
			Func<ICriticalErrorContext, Task> onCriticalError)
		{
			var endpointConfiguration = new EndpointConfiguration(endpointName);

			// setup general
			endpointConfiguration.UsePersistence<LearningPersistence>();
			endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
			endpointConfiguration.DefineCriticalErrorAction(onCriticalError);
			endpointConfiguration.EnableInstallers();

			// setup auditing
			endpointConfiguration.SendFailedMessagesTo("error");
			endpointConfiguration.AuditProcessedMessagesTo("audit");

			// setup transport
			var transportExtensions = endpointConfiguration.UseTransport<RabbitMQTransport>();
			transportExtensions.UseConventionalRoutingTopology();
			transportExtensions.ConnectionString(transportConnectionString);

			return endpointConfiguration;
		}
	}
}
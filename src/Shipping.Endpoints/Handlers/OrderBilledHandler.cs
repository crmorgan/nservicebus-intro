using Billing.Messages.Events;
using NServiceBus;
using NServiceBus.Logging;
using System.Threading.Tasks;

namespace Shipping.Endpoints.Handlers
{
	public class OrderBilledHandler : IHandleMessages<OrderBilled>
	{
		private static readonly ILog Log = LogManager.GetLogger<OrderBilledHandler>();

		public Task Handle(OrderBilled message, IMessageHandlerContext context)
		{
			Log.Info($"******************* Received OrderBilled, OrderId = {message.OrderId} - Should we ship now? ******************");
			return Task.CompletedTask;
		}
	}
}
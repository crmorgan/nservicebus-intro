using System.Threading.Tasks;
using Billing.Messages.Events;
using NServiceBus;
using NServiceBus.Logging;

namespace Shipping.Endpoints
{
	public class OrderBilledHandler : IHandleMessages<OrderBilled>
	{
		private static readonly ILog Log = LogManager.GetLogger<OrderBilledHandler>();

		public Task Handle(OrderBilled message, IMessageHandlerContext context)

		{
			Log.Info($"******************* Received OrderBilled, OrderId = {message.OrderId} ******************");

			// Should this be shipped yet?
			return Task.CompletedTask;
		}
	}
}

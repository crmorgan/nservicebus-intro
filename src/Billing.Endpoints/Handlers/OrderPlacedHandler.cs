using Billing.Messages.Events;
using NServiceBus;
using NServiceBus.Logging;
using Sales.Messages.Events;
using System.Threading;
using System.Threading.Tasks;

namespace Billing.Endpoints.Handlers
{
	public class OrderPlacedHandler : IHandleMessages<OrderPlaced>
	{
		private static readonly ILog Log = LogManager.GetLogger<OrderPlacedHandler>();

		public Task Handle(OrderPlaced message, IMessageHandlerContext context)
		{
			Log.Info($"******************** OrderPlaced for order id '{message.OrderId}' ********************");

			// TODO: Load the payment method and amount data from the billing database
			// TODO: This is where you would call out to your Payment Gateway to charge or put hold on the credit card
			Thread.Sleep(5000);

			var orderBilled = new OrderBilled
			{
				OrderId = message.OrderId
			};

			context.Publish(orderBilled);

			return Task.CompletedTask;
		}
	}
}
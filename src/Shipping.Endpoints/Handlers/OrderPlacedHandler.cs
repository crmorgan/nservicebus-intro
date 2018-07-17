using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using Sales.Messages.Events;

namespace Shipping.Endpoints.Handlers
{
	public class OrderPlacedHandler : IHandleMessages<OrderPlaced>
	{
		private static readonly ILog Log = LogManager.GetLogger<OrderPlacedHandler>();

		public Task Handle(OrderPlaced message, IMessageHandlerContext context)
		{
			// TODO: This is where you would load the customers shipping information and process
			Log.Info($"******************** PlaceOrder for order id '{message.OrderId}' ********************");

			return Task.Delay(3000);
		}
	}
}
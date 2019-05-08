using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using Sales.Messages.Commands;
using Sales.Messages.Events;

namespace Sales.Endpoints.Handlers
{
    public class PlaceOrderHandler : IHandleMessages<PlaceOrder>
    {
	    private static readonly ILog Log = LogManager.GetLogger<PlaceOrderHandler>();
		public Task Handle(PlaceOrder message, IMessageHandlerContext context)
	    {
			// TODO: This is where you would load the existing order from the sales database and have logic to place the order

		    Log.Info($"******************** PlaceOrder for order id '{message.OrderId}' ********************");

		    var orderPlacedEvent = new OrderPlaced
		    {
			    OrderId = message.OrderId
		    };

			return context.Publish(orderPlacedEvent);
	    }
    }
}

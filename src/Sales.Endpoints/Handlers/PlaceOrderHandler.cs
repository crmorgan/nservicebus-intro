using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using Sales.Messages.Commands;

namespace Sales.Endpoints.Handlers
{
    public class PlaceOrderHandler : IHandleMessages<PlaceOrder>
    {
	    private static readonly ILog Log = LogManager.GetLogger<PlaceOrderHandler>();
		public Task Handle(PlaceOrder message, IMessageHandlerContext context)
	    {
			// TODO: This is where you would load the existing order from the sales database and have logic to place the order

		    Log.Info($"******************** Handled PlaceOrder for order id '{message.OrderId}' ********************");


		    // TODO: Publish a OrderPlaced event

			return Task.CompletedTask;
	    }
    }
}

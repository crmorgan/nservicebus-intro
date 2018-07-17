using System.Threading.Tasks;
using NServiceBus;
using Sales.Messages.Events;

namespace Billing.Endpoints.Handlers
{
    public class OrderPlacedHandler : IHandleMessages<OrderPlaced>
    {
	    public Task Handle(OrderPlaced message, IMessageHandlerContext context)
	    {
			// TODO: Load the payment method and amount data from the billing database
			// TODO: This is where you would call out to your Payment Gateway to charge or put hold on the credit card
		    return Task.Delay(5000);
	    }
    }
}

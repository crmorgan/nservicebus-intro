using NServiceBus;
using NServiceBus.Logging;
using Sales.Messages.Events;
using System.Threading.Tasks;

namespace Billing.Endpoints
{
    public class OrderPlacedHandler : IHandleMessages<OrderPlaced>
    {
        private static readonly ILog Log = LogManager.GetLogger<OrderPlacedHandler>();

        public async Task Handle(OrderPlaced message, IMessageHandlerContext context)
        {
            Log.Info($"******************** OrderPlaced for order id '{message.OrderId}' ********************");

            // Load the payment method and amount data from the billing database
            // Use Payment Gateway to charge or put hold on the credit card
            await Task.Delay(5000);
        }
    }
}

using System.Threading;
using NServiceBus;
using NServiceBus.Logging;
using Sales.Messages.Events;
using System.Threading.Tasks;
using Billing.Messages.Events;

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
            Thread.Sleep(4000); // simulate a long running call

            var orderBilled = new OrderBilled
                              {
                                  OrderId = message.OrderId
                              };

            await context.Publish(orderBilled);
        }
    }
}

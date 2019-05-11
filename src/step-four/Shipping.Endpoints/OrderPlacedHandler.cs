using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using Sales.Messages.Events;

namespace Shipping.Endpoints
{
    public class OrderPlacedHandler : IHandleMessages<OrderPlaced>
    {
        private static readonly ILog Log = LogManager.GetLogger<OrderPlacedHandler>();

        public async Task Handle(OrderPlaced message, IMessageHandlerContext context)
        {
            Log.Info($"******************** OrderPlaced for order id '{message.OrderId}' ********************");

            // Should this be shipped yet?
            // Load the warehouse data for the products, notify fulfillment agency, etc.
            // Call some third party shipping API like FedEx to schedule a pickup
            await Task.CompletedTask;
        }
    }
}

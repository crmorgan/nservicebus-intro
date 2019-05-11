using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using Shipping.Messages.Commands;
using Shipping.Messages.Events;

namespace Shipping.Endpoints
{
    public class ShipOrderHandler : IHandleMessages<ShipOrder>
    {
        private static readonly ILog Log = LogManager.GetLogger<ShipOrderHandler>();

        public async Task Handle(ShipOrder message, IMessageHandlerContext context)
        {
            Log.Info($"******************* Received ShipOrder, OrderId = {message.OrderId} ******************");

            Thread.Sleep(25000); // cause OrderShippingPickupTimeExceeded to happen before OrderShipped

            var orderShipped = new OrderShipped
                               {
                                   OrderId = message.OrderId
                               };

            await context.Publish(orderShipped);
        }
    }
}

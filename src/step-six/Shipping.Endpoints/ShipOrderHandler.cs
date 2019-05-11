using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using Shipping.Messages.Commands;

namespace Shipping.Endpoints
{
    public class ShipOrderHandler : IHandleMessages<ShipOrder>
    {
        private static readonly ILog Log = LogManager.GetLogger<ShipOrderHandler>();

        public async Task Handle(ShipOrder message, IMessageHandlerContext context)
        {
            Log.Info($"******************* Received ShipOrder, OrderId = {message.OrderId} ******************");

            await Task.CompletedTask;
        }
    }
}

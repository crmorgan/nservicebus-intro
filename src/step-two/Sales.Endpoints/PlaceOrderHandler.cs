using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using Sales.Messages.Commands;
using Sales.Messages.Events;

namespace Sales.Endpoints
{
    public class PlaceOrderHandler : IHandleMessages<PlaceOrder>
    {
        private static readonly ILog Log = LogManager.GetLogger<PlaceOrderHandler>();

        public async Task Handle(PlaceOrder message, IMessageHandlerContext context)
        {
            // This is where you would load the existing order from the sales database and perform your business logic

            Log.Info($"******************** PlaceOrder for order id '{message.OrderId}' ********************");

            await Task.CompletedTask;
        }
    }
}

using NServiceBus;
using NServiceBus.Logging;
using System.Threading.Tasks;

namespace Shipping.Endpoints.Handlers
{
	public class ShipOrderHandler : IHandleMessages<ShipOrder>
	{
		private static readonly ILog Log = LogManager.GetLogger<ShipOrderHandler>();

		public Task Handle(ShipOrder message, IMessageHandlerContext context)
		{
			Log.Info($"******************* Received ShipOrder, OrderId = {message.OrderId} - order shipped ******************");
			return Task.CompletedTask;
		}
	}
}
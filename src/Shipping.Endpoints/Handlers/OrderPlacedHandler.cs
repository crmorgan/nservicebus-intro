using NServiceBus;
using NServiceBus.Logging;
using Sales.Messages.Events;
using System.Threading.Tasks;

namespace Shipping.Endpoints.Handlers
{
	public class OrderPlacedHandler : IHandleMessages<OrderPlaced>
	{
		private static readonly ILog Log = LogManager.GetLogger<OrderPlacedHandler>();

		public Task Handle(OrderPlaced message, IMessageHandlerContext context)
		{
			Log.Info($"******************* Received OrderPlaced, OrderId = {message.OrderId} - Should we ship now? ******************");
			return Task.CompletedTask;
		}
	}
}
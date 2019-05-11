using NServiceBus;
using NServiceBus.Logging;
using Shipping.Messages.Events;
using System;
using System.Threading.Tasks;

namespace Shipping.Endpoints.Handlers
{
	public class ShipOrderHandler : IHandleMessages<ShipOrder>
	{
		private static readonly ILog Log = LogManager.GetLogger<ShipOrderHandler>();

		public async Task Handle(ShipOrder message, IMessageHandlerContext context)
		{
			Log.Info(
				$"******************* Received ShipOrder, OrderId = {message.OrderId} - order shipped ******************");

			var failShipping = false;  // change this to true to demo saga timeout

			if (failShipping)
			{
				throw new Exception($"Unable to ship, OrderId = {message.OrderId}");
			}

			var orderShipped = new OrderShipped
			{
				OrderId = message.OrderId
			};

			await context.Publish(orderShipped);
		}
	}
}
using NServiceBus;

namespace Shipping.Messages.Events
{
	public class OrderShipped : IEvent
	{
		public int OrderId { get; set; }
	}
}
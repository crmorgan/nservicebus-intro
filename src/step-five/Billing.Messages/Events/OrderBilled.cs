using NServiceBus;

namespace Billing.Messages.Events
{
	public class OrderBilled : IEvent
	{
		public int OrderId { get; set; }
	}
}
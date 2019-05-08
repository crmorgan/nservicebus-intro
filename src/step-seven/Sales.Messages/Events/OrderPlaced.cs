using NServiceBus;

namespace Sales.Messages.Events
{
	public class OrderPlaced : IEvent
	{
		public int OrderId { get; set; }
	}
}
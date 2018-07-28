using NServiceBus;

namespace Shipping.Endpoints
{
	public class ShipOrder : ICommand
	{
		public int OrderId { get; set; }
	}
}
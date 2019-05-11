using NServiceBus;

namespace Shipping.Messages.Commands
{
    public class ShipOrder : ICommand
    {
        public int OrderId { get; set; }
    }
}

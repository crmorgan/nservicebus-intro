using NServiceBus;

namespace Sales.Messages.Commands
{
    public class PlaceOrder : ICommand
    {
	    public int OrderId { get; set; }
    }
}

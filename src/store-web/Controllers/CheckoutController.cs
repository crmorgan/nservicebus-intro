using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NServiceBus;
using Sales.Messages.Commands;

namespace store_web.Controllers
{
    public class CheckoutController : Controller
    {
	    private readonly IMessageSession _bus;

	    public CheckoutController(IMessageSession bus)
	    {
		    _bus = bus;
	    }


        public IActionResult Index()
        {
			TempData["OrderId"] = GenerateOrderId();

            return View();
        }

		public async Task<IActionResult> PlaceOrder(int orderId)
		{
			TempData["OrderId"] = orderId;

			var placeOrderCommand = new PlaceOrder
			{
				OrderId = orderId
			};

			await _bus.Send(placeOrderCommand);

			return View("Confirmation");
		}

		public IActionResult Confirmation()
		{
			return View();
		}


		private int GenerateOrderId()
		{
			var random = new Random();
			return random.Next(1, 1000);
		}
	}
}
using System;
using Microsoft.AspNetCore.Mvc;

namespace store_web.Controllers
{
    public class CheckoutController : Controller
    {
        public IActionResult Index()
        {
			TempData["OrderId"] = GenerateOrderId();

            return View();
        }

		public IActionResult PlaceOrder(int orderId)
		{
			TempData["OrderId"] = orderId;

			// TODO: send a PlaceOrder command to the bus

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
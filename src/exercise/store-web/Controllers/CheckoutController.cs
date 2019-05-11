using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NServiceBus;
using NServiceBus.Logging;

namespace store_web.Controllers
{
    public class CheckoutController : Controller
    {
        private static readonly ILog Log = LogManager.GetLogger<CheckoutController>();
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

        public IActionResult PlaceOrder(int orderId)
        {
            TempData["OrderId"] = orderId;

            Log.Info($"******************** Sending PlaceOrder command for order id '{orderId}' ********************");

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

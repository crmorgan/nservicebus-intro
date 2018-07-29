using Billing.Messages.Events;
using NServiceBus;
using NServiceBus.Logging;
using Sales.Messages.Events;
using Shipping.Messages.Events;
using System;
using System.Threading.Tasks;

namespace Shipping.Endpoints
{
	public class ShippingPolicy : Saga<ShippingPolicy.ShippingPolicyData>,
		IAmStartedByMessages<OrderPlaced>,
		IAmStartedByMessages<OrderBilled>,
		IAmStartedByMessages<OrderShipped>,
		IHandleTimeouts<ShippingPolicy.OrderShippingLate>
	{
		private static readonly ILog Log = LogManager.GetLogger<ShippingPolicy>();

		public Task Handle(OrderPlaced message, IMessageHandlerContext context)
		{
			Log.Info($"******************* Received OrderPlaced, OrderId = {message.OrderId} - Should we ship now? ******************");
			Data.IsOrderPlaced = true;
			return ProcessOrder(context);
		}

		public Task Handle(OrderBilled message, IMessageHandlerContext context)
		{
			Log.Info($"******************* Received OrderBilled, OrderId = {message.OrderId} - Should we ship now? ******************");
			Data.IsOrderBilled = true;
			return ProcessOrder(context);
		}

		public Task Handle(OrderShipped message, IMessageHandlerContext context)
		{
			Log.Info($"******************* Received OrderShipped, OrderId = {message.OrderId} ******************");
			Data.IsOrderShipped = true;
			return ProcessOrder(context);
		}

		public Task Timeout(OrderShippingLate state, IMessageHandlerContext context)
		{
			Log.Info($"******************* Received OrderShippingLate timeout, OrderId = {Data.OrderId} ******************");
			return Task.CompletedTask;
		}

		private async Task ProcessOrder(IMessageHandlerContext context)
		{
			if (Data.IsOrderPlaced && Data.IsOrderBilled && !Data.IsOrderShipped)
			{
				Log.Info($"******************* Processing order, OrderId = {Data.OrderId} - Order can now be shipped! ******************");

				await context.SendLocal(new ShipOrder { OrderId = Data.OrderId });
				await RequestTimeout<OrderShippingLate>(context, TimeSpan.FromSeconds(10));
			}
			else if (Data.IsOrderPlaced && Data.IsOrderBilled && Data.IsOrderShipped)
			{
				Log.Info($"******************* Shipping completed, OrderId = {Data.OrderId} ******************");
				MarkAsComplete();
			}
		}

		protected override void ConfigureHowToFindSaga(SagaPropertyMapper<ShippingPolicyData> mapper)
		{
			mapper.ConfigureMapping<OrderPlaced>(message => message.OrderId)
				.ToSaga(sagaData => sagaData.OrderId);

			mapper.ConfigureMapping<OrderBilled>(message => message.OrderId)
				.ToSaga(sagaData => sagaData.OrderId);

			mapper.ConfigureMapping<OrderShipped>(message => message.OrderId)
				.ToSaga(sagaData => sagaData.OrderId);
		}

		public class ShippingPolicyData : ContainSagaData
		{
			public int OrderId { get; set; }
			public bool IsOrderPlaced { get; set; }
			public bool IsOrderBilled { get; set; }
			public bool IsOrderShipped { get; set; }
		}

		public class OrderShippingLate {}
	}
}
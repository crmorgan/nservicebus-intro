using System;
using System.Threading.Tasks;
using Billing.Messages.Events;
using NServiceBus;
using NServiceBus.Logging;
using Sales.Messages.Events;
using Shipping.Messages.Commands;
using Shipping.Messages.Events;

namespace Shipping.Endpoints
{
    public class ShippingPolicy : Saga<ShippingPolicyData>, 
                                  IAmStartedByMessages<OrderPlaced>, 
                                  IAmStartedByMessages<OrderBilled>,
                                  IHandleMessages<OrderShipped>,
                                  IHandleTimeouts<OrderShippingPickupTimeExceeded>
    {
        private static readonly ILog Log = LogManager.GetLogger<ShippingPolicy>();

        public Task Handle(OrderPlaced message, IMessageHandlerContext context)
        {
            Log.Info($"******************* Received OrderPlaced, OrderId = {message.OrderId} ******************");
            Data.IsOrderPlaced = true;
            return ProcessOrder(context);
        }

        public Task Handle(OrderBilled message, IMessageHandlerContext context)
        {
            Log.Info($"******************* Received OrderBilled, OrderId = {message.OrderId} ******************");
            Data.IsOrderBilled = true;
            return ProcessOrder(context);
        }

        public Task Handle(OrderShipped message, IMessageHandlerContext context)
        {
            Log.Info($"******************* Received OrderShipped, OrderId = {message.OrderId} ******************");
            Data.IsShipped = true;
            return ProcessOrder(context);
        }
		
		public async Task Timeout(OrderShippingPickupTimeExceeded state, IMessageHandlerContext context)
        {
            Log.Info($"******************* Received OrderShipped, OrderId = {Data.OrderId} ******************");
			// Have secondary carrier to ship the order
            MarkAsComplete();
            await Task.CompletedTask;
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

        private async Task ProcessOrder(IMessageHandlerContext context)
        {
            if (Data.IsOrderPlaced && Data.IsOrderBilled && Data.IsShipped)
            {
                await context.SendLocal(new ShipOrder { OrderId = Data.OrderId });
                await RequestTimeout<OrderShippingPickupTimeExceeded>(context, TimeSpan.FromSeconds(20));
            }
        }
    }

    public class OrderShippingPickupTimeExceeded
    {
    }

    public class ShippingPolicyData : ContainSagaData
    {
        public int OrderId { get; set; }
        public bool IsOrderPlaced { get; set; }
        public bool IsOrderBilled { get; set; }
        public bool IsShipped { get; set; }
    }
}

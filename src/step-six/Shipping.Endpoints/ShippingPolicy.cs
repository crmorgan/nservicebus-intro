using System.Threading.Tasks;
using Billing.Messages.Events;
using NServiceBus;
using NServiceBus.Logging;
using Sales.Messages.Events;
using Shipping.Messages.Commands;

namespace Shipping.Endpoints
{
    public class ShippingPolicy : Saga<ShippingPolicyData>, IAmStartedByMessages<OrderPlaced>, IAmStartedByMessages<OrderBilled>
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

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<ShippingPolicyData> mapper)
        {
            mapper.ConfigureMapping<OrderPlaced>(message => message.OrderId)
                  .ToSaga(sagaData => sagaData.OrderId);

            mapper.ConfigureMapping<OrderBilled>(message => message.OrderId)
                  .ToSaga(sagaData => sagaData.OrderId);
        }

        private async Task ProcessOrder(IMessageHandlerContext context)
        {
            if (Data.IsOrderPlaced && Data.IsOrderBilled)
            {
                await context.SendLocal(new ShipOrder { OrderId = Data.OrderId });
                MarkAsComplete();
            }
        }
    }

    public class ShippingPolicyData : ContainSagaData
    {
        public int OrderId { get; set; }
        public bool IsOrderPlaced { get; set; }
        public bool IsOrderBilled { get; set; }
    }
}

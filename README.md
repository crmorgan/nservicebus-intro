# NServiceBus Retail Demo with .NET Core
**With .NET Core, Docker, and RabbitMQ**

This is a sample project that gives you and introduction into to building distributed systems with NServiceBus.  The project is based on Particular's tutorial at https://docs.particular.net/tutorials/intro-to-nservicebus/3-multiple-endpoints but is using NServiceBus version 7, Docker containers, and RabbitMQ as the transport.

An accompanying slide deck that introduces some of the concepts of distributed systems and Service Oriented Architecure is also available at https://www.slideshare.net/ChrisMorgan8/introduction-to-microservices-with-nservice-bus.

Each of the steps in this exercise have fully completed source code or you can start with the solution in the `exercise` folder and code along.

### Prerequisites ###
- Windows 10
- Visual Studio 2017 or higher (community edition is fine)
- Docker for Windows with Linux container support enabled

### Start RabbitMQ ###
This exercise will be using RabbitMQ which you can run in a Docker container using the supplied Docker Compose file.

1. Open a command or powershell prompt and change to the root directory of where you cloned the repository to.
2. Run `docker-compose up -d` to start the container in a detached mode.

You can now access the RabbitMQ Management console at http://localhost:15672 and login as the`retaildemo` with password `password`.

To stop RabbitMQ run the `docker-compose down` command.

# Step One #
This step starts you out with a web site named store-web that simulates a shopping cart checkout.  Follow the steps below to run the web site and place on order.

1. Open the retail-demo solution located the `nservicebus-intro/src/exercise` folder in Visual Studio
2. Set the **docker-compose** project as the startup project for the solution
3. Run the project in Visual Studio with <kbd>F5</kbd> and access the store web site at http://localhost:32773/
4. Click the **proceed to checkout** button to go to the checkout page.  Here you will see that there is an order id that has been generated that will be sent to the `PlaceOrder` command that will be added in step two.
5. Click **Place your order** to see that the `CheckoutController.PlaceOrder()` action method is called and the confirmation page is loaded.

### NServiceBus Endpoint Configuration ###
The store-web project has been setup as an NServiceBus endpoint host.  Open the `store-web/Startup.cs` file and review the `AddEndpoint` method to see how an NServiceBus `EndpointConfiguration` object is instanciated and configured.  For details on what is going on here checkout https://docs.particular.net/samples/endpoint-configuration/.

In this demo there will be a few projects that will have a similar configuration and to make this simpler I have encapsulated this in the `Infrastructure.EndpointConfigurationBuilder` class.

Replace the `AddEndpoint` method with the following code to use the builder:

```cs
private void AddEndpoint(IServiceCollection services)
{
    Log.Info("****************** Store website endpoint starting ******************");

    var connectionString = Environment.GetEnvironmentVariable("servicebus_connection_string");
    var endpointConfiguration = new EndpointConfigurationBuilder("store-web", connectionString)
                                        .AsSendOnly()
                                        .Build();
    
    // start the endpoint
    var endpointInstance = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();
    Log.Info("****************** Store website endpoint successfully started ******************");

    // register endpoint instance with the IoC framework
    services.AddSingleton<IMessageSession>(endpointInstance);
}
```

You can remove the `NServiceBus.RabbitMQ` NuGet package from the store-web project if you like since that will now come from the `Infrastructure.EndpointConfigurationBuilder` project.

> As of 5/10/2019 the Particular dotnet new template is using NSB version 7.1.4 and NServiceBus.Newtonsoft.Json version 2.1.0 and is compatible with the version of NServiceBus and NServiceBus.RabbitMQ version 5.0.2 used in the Infrastructure project.  If the NServiceBus version your Particular dotnet new template uses is differnt than this you may have to upgrade/downgrade the Infrastructure project's NServiceBus version so they are compatible.

# Step Two #
In this step you will add a Sales microservice that handles a `PlaceOrder` command sent by the web site's checkout process.
s
### Create a Sales Endpoint ###
Here you are going to use the dotnet CLI and NServiceBus project templates to create a project that will be the enpoint host for the Sales microservice.

1. Open a command prompt, Package Manager Console in Visual Studio, or your prefered command shell.
2. Run `dotnet new -i "ParticularTemplates"` to install the latest version of the [Particular dotnet new Templates](https://docs.particular.net/nservicebus/dotnet-templates).
3. Change to the `nservicebus-intro/src/exercise` directory and run the command `dotnet new nsbdockercontainer -n Sales.Endpoints` to create the new Sales.Endpoints project.
4. In Visual Studio add the project to the `Sales` solution folder using Add Existing Project.  There is already a `Sales.Messages` project in the `Sales` solution folder.  This is a regular netstandard class library project that will be used later on in this step.

### Configure NServiceBus Endpoint ###
In the `Hosts.cs` file:

1. Change the `EndpointName` property value to `sales`
   ```cs
   public string EndpointName => "sales";
   ```
2. Replace the try block in the `Start` method with the following code:
    ```cs
    try
    {
        var connectionString = Environment.GetEnvironmentVariable("servicebus_connection_string");
        var endpointConfiguration = new EndpointConfigurationBuilder(EndpointName, connectionString).Build();

        // start the endpoint
        endpoint = await Endpoint.Start(endpointConfiguration);
        log.Info("****************** Sales endpoint successfully started ******************");
    }  
    ```

### Configure Docker ###
This project is using Docker Compose to run all the containers when you run the solution.  The new Sales.Endpoints project was already configured to be a Docker container when it was created so all you have to now is add it to the solutions docker-compose file.

1. Open the `nservicebus-intro/src/exercise/docker-compose.yaml` file and add the following configuration in the services node at the same level as the store-web service configuration:

    ```yaml
    sales:
        image: sales
        build:
        context: .
        dockerfile: Sales.Endpoints/Dockerfile
        environment:
        servicebus_connection_string: ${servicebus_connection_string}
    ```

The `servicebus_connection_string` is an environment variable that is set in the `nservicebus-intro/src/exercise/.env` file.

### Handle the PlaceOrder Command ###
1. Create a `Sales.Endpoints.PlaceOrderHandler` class that implements `NServiceBus.IHandleMessages<PlaceOrder>`.  The `PlaceOrder` class is in the `Sales.Messages` project.
2. Add a `Handle` method.
    ```cs
    public class PlaceOrderHandler : IHandleMessages<PlaceOrder>
    {
	    private static readonly ILog Log = LogManager.GetLogger<PlaceOrderHandler>();
		
        public async Task Handle(PlaceOrder message, IMessageHandlerContext context)
        {
            // This is where you would load the existing order from the sales database and perform your business logic

            Log.Info($"******************** PlaceOrder for order id '{message.OrderId}' ********************");

            await Task.CompletedTask;
        }
    }
    ```
### Send the ShipOrder Command ###
1. Update the store-web endpoint configuration in `Startup.cs` so it routes `PlaceOrder` command messages to the `sales` endpoint.
    ```cs
        var endpointConfiguration = new EndpointConfigurationBuilder("store-web", connectionString)
                                            .AsSendOnly()
                                            .RouteToEndpoint(typeof(PlaceOrder), "sales")
                                            .Build();
    ```
2. Update the `CheckoutController.PlaceOrder` action method to send the `PlaceOrder` command when the place order button is clicked.
    ```cs
    public async Task<IActionResult> PlaceOrder(int orderId)
    {
        // ...

        var placeOrderCommand = new PlaceOrder
                                {
                                    OrderId = orderId
                                };

        await _bus.Send(placeOrderCommand).ConfigureAwait(false);

        return View("Confirmation");
    }
    ```

Run the solution and place an order using the web site.

# Step Three #
This step adds a new Billing microservice that processes payments for an order. The Billing service will subscribe to an `OrderPlaced` event that is published by the Sales service.

### Create a Billing Endpoint Project ###

1. Open a command window to the `\src\step-one` directory and run the command: `dotnet new nsbdockercontainer -n Billing.Endpoints` 
1. In Visual Studio add the created project to the `Billing` solution folder.

### Configure NServiceBus Endpoint ###
In the `Hosts.cs` file:

1. Change the `EndpointName` property value to `billing`
   ```cs
   public string EndpointName => "billing";
   ```
2. Replace the try block in the `Start` method with the following code:
    ```cs
    try
    {
        var connectionString = Environment.GetEnvironmentVariable("servicebus_connection_string");
        var endpointConfiguration = new EndpointConfigurationBuilder(EndpointName, connectionString).Build();

        // start the endpoint
        endpoint = await Endpoint.Start(endpointConfiguration);
        log.Info("****************** Billing endpoint successfully started ******************");
    }  
    ```

### Configure Docker ###
Add a `billing` service to the `nservicebus-intro/src/exercise/docker-compose.yaml` file.

1. Open the `nservicebus-intro/src/exercise/docker-compose.yaml` file and add the following configuration in the services node:
    ```yaml
    billing:
        image: billing
        build:
        context: .
        dockerfile: Billing.Endpoints/Dockerfile
        environment:
        servicebus_connection_string: ${servicebus_connection_string}
    ```

### Publish the OrderPlaced Event ###
1. Edit the `Sales.Endpoints.PlaceOrderHandler` handler to publish an `OrderPlaced` event.

    ```cs
    public async Task Handle(PlaceOrder message, IMessageHandlerContext context)
    {
        // ...

        var orderPlacedEvent = new OrderPlaced
                                {
                                    OrderId = message.OrderId
                                };

        await context.Publish(orderPlacedEvent);
    }
    ```

### Handle the OrderPlaced Event ###
1. Create a `Billing.Endpoints.OrderPlacedHandler` class that implements `NServiceBus.IHandleMessages<OrderPlaced>`.  The `OrderPlaced` class is in the `Sales.Messages` project.
1. Add a `Handle` method.
   
    ```cs
    public class OrderPlacedHandler : IHandleMessages<OrderPlaced>
    {
        private static readonly ILog Log = LogManager.GetLogger<OrderPlacedHandler>();

        public async Task Handle(OrderPlaced message, IMessageHandlerContext context)
        {
            Log.Info($"******************** OrderPlaced for order id '{message.OrderId}' ********************");

            // Load the payment method and amount data from the billing database
            // Use Payment Gateway to charge or put hold on the credit card
            await Task.CompletedTask;
        }
    }
    ```
The Billing service is now handling the `OrderPlaced` event and will charge the customer's credit card.

# Step Four #
In this step you are going to create a Shipping microservice that will also handle the `OrderPlaced` event which demonstrates having multiple endpoints handling a single event.

### Create a Shipping Endpoint Project ###

1. Change to the `nservicebus-intro/src/exercise` directory and run the command `dotnet new nsbdockercontainer -n Shipping.Endpoints` to create the new Shipping.Endpoints project.
2. Add the NServiceBus.RabbitMQ package to the Billing.Endpoints project: `Install-Package NServiceBus.RabbitMQ`
3. In Visual Studio add the created project to the Billing solution folder.

### Configure NServiceBus Endpoint ###
In the `Hosts.cs` file:

1. Change the `EndpointName` property value to `shipping`
   ```cs
   public string EndpointName => "shipping";
   ```
2. Replace the try block in the `Start` method with the following code:
    ```cs
    try
    {
        var connectionString = Environment.GetEnvironmentVariable("servicebus_connection_string");
        var endpointConfiguration = new EndpointConfigurationBuilder(EndpointName, connectionString).Build();

        // start the endpoint
        endpoint = await Endpoint.Start(endpointConfiguration);
        log.Info("****************** Shipping endpoint successfully started ******************");
    }  
    ```

### Configure Docker ###
Add a `shipping` service to the `nservicebus-intro/src/exercise/docker-compose.yaml` file.

1. Open the `nservicebus-intro/src/exercise/docker-compose.yaml` file and add the following configuration in the services node:
    ```yaml
    shipping:
        image: shipping
        build:
        context: .
        dockerfile: Shipping.Endpoints/Dockerfile
        environment:
        servicebus_connection_string: ${servicebus_connection_string}
    ```

### Handle the OrderPlaced Event ###
1. Create a `Shipping.Endpoints.OrderPlacedHandler` class that implements `NServiceBus.IHandleMessages<OrderPlaced>`.  The `OrderPlaced` class is in the `Sales.Messages` project.
1. Add a `Handle` method.
   
    ```cs
    public class OrderPlacedHandler : IHandleMessages<OrderPlaced>
    {
        private static readonly ILog Log = LogManager.GetLogger<OrderPlacedHandler>();

        public async Task Handle(OrderPlaced message, IMessageHandlerContext context)
        {
            Log.Info($"******************** OrderPlaced for order id '{message.OrderId}' ********************");

            // Should this be shipped yet?
            // Load the warehouse data for the products, notify fulfillment agency, etc.
            // Call some third party shipping API like FedEx to schedule a pickup
            await Task.CompletedTask;
        }
    }
    ```

# Step Five #
This step adds publishing of an `OrderBilled` event by the **Billing** service which will be handled by the **Shipping** service.

In the `Billing.Endpoints.OrderPlacedHandler` class update the `Handle` method so it publishes an `OrderBilled` event after the fake payment processing call.  The `OrderBilled` event has already been created in the **Billing.Messages** project.

    ```cs
        public async Task Handle(OrderPlaced message, IMessageHandlerContext context)
        {
            // ...
           Thread.Sleep(4000); // simulate a long running call

            var orderBilled = new OrderBilled
                            {
                                OrderId = message.OrderId
                            };

            await context.Publish(orderBilled);
        }
    ```

Update **Shipping** to handle the `OrderBilled` event.

1. In the **Shipping.Endpoints** project add a class named `OrderBilledHandler`.
    ```cs
    public class OrderBilledHandler : IHandleMessages<OrderBilled>
    {
        private static readonly ILog Log = LogManager.GetLogger<OrderBilledHandler>();

        public Task Handle(OrderBilled message, IMessageHandlerContext context)

        {
            Log.Info($"******************* Received OrderBilled, OrderId = {message.OrderId} ******************");

            // Should this be shipped yet?
            return Task.CompletedTask;
        }
    }
    ```

## When Should The Order Be Shipped? ##
The **Shipping** service has a problem!  It shouldn't ship the order until it has received both the  `OrderPlaced` **and** `OrderBilled` events but in an eventually consistent system like this the `OrderBilled` event could arrive before `OrderPlaced`.  The **Shipping** service needs to track what events have arrived using a [Saga](https://docs.particular.net/nservicebus/sagas/).

# **Sagas** #

# Step Six #
This step is based on the [NServiceBus sagas: Getting started](https://docs.particular.net/tutorials/nservicebus-sagas/1-getting-started/) tutorial.

In this step we are replacing the `OrderPlacedHandler` and `OrderBilledHandler` handlers in the **Shipping** service with a Saga.

## Refactor Existing Shipping Handlers ##
Move the two handlers into a single handler called `ShippingPolicy`.
  
1. Create a `ShippingPolicy` class in the **Shipping.Endpoints** project and configure it to handle the `OrderPlaced` and `OrderBilled` events.
    ```cs
    public class ShippingPolicy : IHandleMessages<OrderPlaced>, IHandleMessages<OrderBilled>
    {
        private static readonly ILog Log = LogManager.GetLogger<ShippingPolicy>();

        public Task Handle(OrderPlaced message, IMessageHandlerContext context)
        {
            Log.Info($"******************* Received OrderPlaced, OrderId = {message.OrderId} ******************");
            return Task.CompletedTask;
        }

        public Task Handle(OrderBilled message, IMessageHandlerContext context)
        {
            Log.Info($"******************* Received OrderBilled, OrderId = {message.OrderId} ******************");
            return Task.CompletedTask;
        }
    }
    ```
2. Delete the existing two handler classes in the Shipping.Endpoints folder.

### Saga Data ###
Sagas persist data using a [persister](https://docs.particular.net/persistence/) that is registered with the endpoint.  The `Infrastructure.EndpointConfigurationBuilder` is already doing this for you and is using the [LearningPersister](https://docs.particular.net/persistence/learning/) which simulates Saga persistence.

1. Create a `ShippingPolicyData` class that has properties to track whether or not the `OrderPlaced` and `OrderBilled` events have been received.  Since this data is only used by the Saga it can be an internal class to `ShippingPolicy`.
    ```cs
    public class ShippingPolicy : Saga<ShippingPolicyData>, IAmStartedByMessages<OrderPlaced>, IAmStartedByMessages<OrderBilled>
    {
        // ...

        public class ShippingPolicyData : ContainSagaData
        {
            public string OrderId { get; set; }
            public bool IsOrderPlaced { get; set; }
            public bool IsOrderBilled { get; set; }
        }
    }
    ```
2. Make `ShippingPolicy` a Saga that uses `ShippingPolicyData` by having it inherit from from `Saga<ShippingPolicy.ShippingPolicyData>` and t
    ```cs
    public class ShippingPolicy : Saga<ShippingPolicyData>, IHandleMessages<OrderPlaced>, IHandleMessages<OrderBilled>
    ```
3. Implement the abstract `ConfigureHowToFindSaga` member and tell the Saga how messages are correlated to a Saga instance.
    ```cs
    protected override void ConfigureHowToFindSaga(SagaPropertyMapper<ShippingPolicyData> mapper)
    {
        mapper.ConfigureMapping<OrderPlaced>(message => message.OrderId)
            .ToSaga(sagaData => sagaData.OrderId);

        mapper.ConfigureMapping<OrderBilled>(message => message.OrderId)
            .ToSaga(sagaData => sagaData.OrderId);
    }
    ```
4. Update both `Handle` methods so they set the corresponding flag on the `ShippingPolicyData` class using the Saga'a `Data` property.
    ```cs
    public Task Handle(OrderPlaced message, IMessageHandlerContext context)
    {
        Log.Info($"******************* Received OrderPlaced, OrderId = {message.OrderId} ******************");
        Data.IsOrderPlaced = true;
        return Task.CompletedTask;
    }

    public Task Handle(OrderBilled message, IMessageHandlerContext context)
    {
        Log.Info($"******************* Received OrderBilled, OrderId = {message.OrderId} ******************");
        Data.IsOrderBilled = true;
        return Task.CompletedTask;
    }
    ```
5. Configure the Saga so it is started by both the `OrderPlaced` or `OrderBilled` events by using the `IAmStartedByMessages<T>` interface instead of `IHandleMessages<T>`.
    ```cs
    public class ShippingPolicy : Saga<ShippingPolicyData>, IAmStartedByMessages<OrderPlaced>, IAmStartedByMessages<OrderBilled>
    ```
6. Add a `ProcessOrder` method to the `ShippingPolicy` class that will send a `ShipOrder` command and mark the Saga as complete if the order has been placed and billed.
    ```cs
    private async Task ProcessOrder(IMessageHandlerContext context)
    {
        if (Data.IsOrderPlaced && Data.IsOrderBilled)
        {
            await context.SendLocal(new ShipOrder { OrderId = Data.OrderId });
            MarkAsComplete();
        }
    }
    ```
7. Update both `Handle` methods to call the new `ProcessOrder` method like this.
   ```cs
    public Task Handle(OrderPlaced message, IMessageHandlerContext context)
    {
        Log.Info($"******************* Received OrderPlaced, OrderId = {message.OrderId} ******************");
        Data.IsOrderPlaced = true;
        return ProcessOrder(context);
    }
    ```
8. Add a `ShipOrderHandler` class to the `Shipping.Endpoints` project that will handle the `ShipOrder` command and publish an `OrderShipped` event.
    ```cs
    public class ShipOrderHandler : IHandleMessages<ShipOrder>
    {
        private static readonly ILog Log = LogManager.GetLogger<ShipOrderHandler>();

        public async Task Handle(ShipOrder message, IMessageHandlerContext context)
        {
            Log.Info($"******************* Received ShipOrder, OrderId = {message.OrderId} ******************");
            await Task.CompletedTask;
        }
    }
    ```

### Important note about Sagas: ###
> "Other than interacting with its own internal state, a saga should not access a database, call out to web services, or access other resources - neither directly nor indirectly by having such dependencies injected into it."

# Step Seven #
The shipping carrier we use can sometimes has issues getting our shipments out on time and our customers need to so we want to update our Saga so that if an order is not shipped within ten seconds some action should be taken like notifying the customer that their order is delayed.  We can do this with a [Saga Timeout](https://docs.particular.net/nservicebus/sagas/timeouts "Saga Timeout") which is essentialy a way to have an event be scheduled for some time in the future.
 
This step adds a timeout to the `ShippingPolicy` Saga called `OrderShippingPickupTimeExceeded`.

1. In `ShipOrderHandler` publish a `OrderShipped` event
    ```cs
    public class ShipOrderHandler : IHandleMessages<ShipOrder>
    {
        private static readonly ILog Log = LogManager.GetLogger<ShipOrderHandler>();

        public async Task Handle(ShipOrder message, IMessageHandlerContext context)
        {
            Log.Info($"******************* Received ShipOrder, OrderId = {message.OrderId} ******************");

            var orderShipped = new OrderShipped
            {
                OrderId = message.OrderId
            };

            await context.Publish(orderShipped);
        }
    }
    ```
2. Update `ShippingPolicy` Saga to be handle the `OrderShipped` event 
    ```cs
        public class ShippingPolicy : Saga<ShippingPolicyData>, 
                                        IAmStartedByMessages<OrderPlaced>, 
                                        IAmStartedByMessages<OrderBilled>,
                                        IHandleMessages<OrderShipped>
    ```
3. Implement the Handle method and set an `IsShipped` flag on the Saga data object. 
    ```cs
    public Task Handle(OrderShipped message, IMessageHandlerContext context)
    {
        Log.Info($"******************* Received OrderShipped, OrderId = {message.OrderId} ******************");
        Data.IsShipped = true;
        return ProcessOrder(context);
    }
    ```
4. Update the `ProcessOrder` method to check `IsShipped`
    ```cs
    private async Task ProcessOrder(IMessageHandlerContext context)
    {
        if (Data.IsOrderPlaced && Data.IsOrderBilled && Data.IsShipped)
        {
            await context.SendLocal(new ShipOrder { OrderId = Data.OrderId });
            MarkAsComplete();
        }
    }
    ```
5. Update the `ConfigureHowToFindSaga` method so the Saga knows how to map to the `OrderShipped` event.  
    ```cs
    protected override void ConfigureHowToFindSaga(SagaPropertyMapper<ShippingPolicyData> mapper)
    {
        // ...

        mapper.ConfigureMapping<OrderShipped>(message => message.OrderId)
            .ToSaga(sagaData => sagaData.OrderId);
    }
    ```
6. Update the Saga to handle the timeout
    ```cs
    public class ShippingPolicy : Saga<ShippingPolicyData>, 
                                  IAmStartedByMessages<OrderPlaced>, 
                                  IAmStartedByMessages<OrderBilled>,
                                  IHandleMessages<OrderShipped>,
                                  IHandleTimeouts<OrderShippingPickupTimeExceeded>
    ```
7. Implement the method to handle the timeout
```cs

```
8. In the `ProcessOrder` method request a `OrderShippingLate` timeout after the `ShipOrder` command is sent and remove the `MarkAsComplete()` call.
    ```cs
    private async Task ProcessOrder(IMessageHandlerContext context)
    {
        if (Data.IsOrderPlaced && Data.IsOrderBilled && Data.IsShipped)
        {
            await context.SendLocal(new ShipOrder { OrderId = Data.OrderId });
            await RequestTimeout<OrderShippingPickupTimeExceeded>(context, TimeSpan.FromSeconds(20));
        }
    }
    ```
9.  Add a `Timeout` method that will handle the `OrderShippingPickupTimeExceeded` event.
    ```cs
    public async Task Timeout(OrderShippingPickupTimeExceeded state, IMessageHandlerContext context)
    {
        Log.Info($"******************* Received OrderShipped, OrderId = {Data.OrderId} ******************");
        MarkAsComplete();
        await Task.CompletedTask;
    }
    ```
10. Add  a `OrderShippingPickupTimeExceeded` as an inner class of `ShippingPolicy`
    ```cs
    public class OrderShippingPickupTimeExceeded {}
    ```
11. Update `ShipOrderHandler` to have a delay or throw an exception so the `OrderShippingPickupTimeExceeded` event is handled before `OrderShipped` is.
    ```cs
    public async Task Handle(ShipOrder message, IMessageHandlerContext context)
    {
        Log.Info($"******************* Received ShipOrder, OrderId = {message.OrderId} ******************");

        Thread.Sleep(25000); // cause OrderShippingPickupTimeExceeded to happen before OrderShipped

        var orderShipped = new OrderShipped
                        {
                            OrderId = message.OrderId
                        };

        await context.Publish(orderShipped);
    }
    ```
12. In the Shipping `Host.cs` disable delayed and immediate retries to make it easier to demo

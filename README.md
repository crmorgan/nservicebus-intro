# NServiceBus Retail Demo with .NET Core
**With .NET Core, Docker, and RabbitMQ**

This is a sample project that gives you and introduction into to building distributed systems with NServiceBus.  The project is based on Particular's tutorial at https://docs.particular.net/tutorials/intro-to-nservicebus/3-multiple-endpoints but is using NServiceBus version 7, Docker containers, and RabbitMQ as the transport.

An accompanying slide deck that introduces some of the concepts of distributed systems and Service Oriented Architecure is also available at https://www.slideshare.net/ChrisMorgan8/introduction-to-microservices-with-nservice-bus.

Each of the steps or stages below are available as branches or you can get the fully implement demo by checking out the HEAD of master.

### Prerequisites ###
- Windows 10
- Visual Studio 2017 (community edition is fine)
- Docker for Windows with Linux container support enabled
- NServiceBus dotnet new templates

### Start RabbitMQ ###
Before running any containers for the website start the RabbitMQ container.

Open a command or powershell prompt and cChange to the root directory of where you cloned the repository to

Run `docker-compose up -d`

You can now access the RabbitMQ Management console at http://localhost:15672

Username: `retaildemo`

Password: `password`

To stop RabbitMQ run `docker-compose down`


## Step One ##
This step starts you out with a working web site that allows you to checkout and place an order but does not publish any events and there are no endpoints yet.

1. Run the project in Visual Studio with <kbd>F5</kbd> to launch the store web site at http://localhost:32773/
2. Click the **proceed to checkout** button to go to the checkout page.  Here you will see that there is an order id that has been generated that will be sent to the `PlaceOrder` command that will be added in step two.
3. Click **Place your order** to see that the `CheckoutController.PlaceOrder()` action method is called and the confirmation page is loaded.  In the next step you will publish the `PlaceOrder` command from this action method.


## Step Two ##
This step adds publishing of the **PlaceOrder** command in the checkout controller and handling of the command in a Sales endpoint.

### Create the Sales Endpoint ###

From a command or bash window

1. Run `dotnet new -i "ParticularTemplates::*"` to install the NServiceBus dotnet new template
2. Change to the `\src` directory and run the command: `dotnet new nsbdockercontainer -n Sales.Endpoints`
3. In Visual Studio add the new project to the `Sales` solution folder
3. Add the NServiceBus.RabbitMQ package to the Sales.Endpoints project: `Install-Package NServiceBus.RabbitMQ`


In the `Hosts.cs` file:

1. Change `EndpointName` to `sales`
2. Edit the `EndpointConfiguration` settings to use `Infrastructure.Endpoint.EndpointConfigurationBuilder`

### Configure Docker ###
1. Add a `shipping` service to the `/src/docker-compose.yaml` file

### Handle the PlaceOrder Command ###
1. Add a `Sales.Endpoints.PlaceOrderHandler` to handle the `PlaceOrder` command


 
## Step Three ##
This steps adds publishing of the`OrderPlaced` event in the `PlaceOrderHandler` and handling it using the Billing endpoint.

### Create a Billing Endpoint Project ###

1. Open a command window to the `\src` directory and run the command: `dotnet new nsbdockercontainer -n Billing.Endpoints` 
2. In Visual Studio add the created project to the `Billing` solution folder.
3. Add the NServiceBus.RabbitMQ package to the `Billing.Endpoints` project: `Install-Package NServiceBus.RabbitMQ`


### Configure NServiceBus ###
In the 'Hosts.cs' file:

1. Change `EndpointName` to `billing`
2. Edit the `EndpointConfiguration` settings to use `Infrastructure.Endpoint.EndpointConfigurationBuilder`

### Configure Docker ###
1. Add a `billing` service to the `/src/docker-compose.yaml` file

### Publish the OrderPlaced Event ###
1. Create an `OrderPlaced` event in `Sales.Messages.Events`
2. Publish the `OrderPlaced` event from the `Sales.Endpoints.PlaceOrderHandler` handler

### Handle the OrderPlaced Event ###
1. Add a `Billing.Endpoints.OrderPlacedHandler` to handle the `OrderPlaced` event



## Step Four ##
This steps adds handling of the `OrderPlaced` event in the Shipping endpoint.

### Create a Shipping Endpoint Project ###

1. Open a command window to the `\src` directory and run the command: `dotnet new nsbdockercontainer -n Shipping.Endpoints`.
2. Add the NServiceBus.RabbitMQ package to the Billing.Endpoints project: `Install-Package NServiceBus.RabbitMQ`
3. In Visual Studio add the created project to the Billing solution folder.

### Configure NServiceBus ###
In the the `Hosts.cs` file

1. Change `EndpointName` to `shipping`
2. Edit the `EndpointConfiguration` settings to use `Infrastructure.Endpoint.EndpointConfigurationBuilder`

### Configure Docker ###
2. Add a `shipping` service to the `/src/docker-compose.yaml` file.

### Create an OrderPlaced Handler ###

1. Add a `Shipping.Endpoints.OrderPlacedHandler` to handle the `OrderPlaced` event


## Step Five ##
This steps adds publishing of `OrderBilled` event and handling of it in the Shipping endpoint.

1. In the `Billing.Endpoints.OrderPlacedHandler` publish an `OrderBilled` event.
2. Create a `Billing.Messages` project with an `Events` directly and add an `OrderBilled` event class.  This event just needs an `OrderId` property.
2. Add a `Shipping.Endpoints.OrderBilledHandler` to handle the `OrderBilled` event

At this point the system publishes the OrderPlaced event which is handled by Shipping and Billing.  Billing is publishing an OrderBilled event which completes the billing part of the order but the order can't be shipped yet because Shipping cannot determine if both the OrderPlaced and OrderBilled events have occurred.  We need a Saga in order to do that which we will do in the next step. 


## Step Six ##
This step is based on the tutorial [NServiceBus sagas: Getting started](https://docs.particular.net/tutorials/nservicebus-sagas/1-getting-started/) tutorial.  In this step we are replacing the `OrderPlaced` and `OrderBilled` event handlers in Shipping with a Saga. 

### Refactor Existing Shipping Handlers ###
We are going to move the two handlers in Shipping into a single handler called ShippingPolicy.
  
1. Create a `ShippingPolicy` class in the Shipping.Endpoints project and configure it to handle the `OrderPlaced` and `OrderBilled` events.

2. Delete the existing two handler classes in the Shipping.Endpoints/Handlers folder.

### Create a Saga ###
1. Create a `ShippingPolicyData` class that has properties to track whether or not the OrderPlaces and OrderBilled events have occurred.  Define this class as an internal class to `ShippingPolicy`.

2. Tell the Saga to use the `ShippingPolicyData` class by inheriting from `Saga<ShippingPolicy.ShippingPolicyData>` and implement the abstract `ConfigureHowToFindSaga` member.

3. Configure the Saga to start by the `OrderPlaced` or `OrderBilled` events.


## Step Seven ##
The shipping carrier we use can sometimes has issues getting our shipments out on time so we want to update our Saga so that if an order is not shipped (i.e., we have not received the `OrderShipped` event) within ten seconds some action should be taken.
 
This step adds a [Saga Timeout](https://docs.particular.net/nservicebus/sagas/timeouts "Saga Timeout") called `OrderShippingLate `.

1. In `ShipOrderHandler` publish an `OrderShipped` event
2. Update `ShippingPolicy` Saga to be started by the `OrderShipped` event and add it to the `ConfigureHowToFindSaga` method.  Add a handle method and set an `IsShipped` value on `ShippingPolicyData` to true.
3. In `ShippingPolicy.ProcessOrder()` move `MarkAsComplete()` inside an new conditional that checks all order requirements have been met.
4. In the `ProcessOrder` method request a `OrderShippingLate` timeout after the `ShipOrder` command is sent.
5. Update the `ShippingPolicy` Saga to handle the Timeout: `IHandleTimeouts<OrderShippingLate>`
6. Update `ShipOrderHandler` to have a delay or throw exception so the `OrderShipped` event is not raised before the timeout period.
7. In the Shipping `Host.cs` disable delayed and immediate retries to make it easier to demo

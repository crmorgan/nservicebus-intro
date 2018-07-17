# nservicebus-intro
Introduction to building distributed systems with NServiceBus retail demo.  This is based on the tutorial at https://docs.particular.net/tutorials/intro-to-nservicebus/3-multiple-endpoints but is running NServiceBus version 7 in Docker containers and RabbitMQ as the transport.

Accompanying slide deck https://www.slideshare.net/ChrisMorgan8/introduction-to-microservices-with-nservice-bus

Each of the steps or stages below are in tagged in master or you can get the fully implement demo by checking out the HEAD of master.

### Prerequisites ###
- Windows 10
- Visual Studio 2017 (community edition is fine)
- Docker for Windows with Linux container support enabled
- Install the NServiceBus .net templates
 - run `dotnet new -i "ParticularTemplates::*"` 

### Start RabbitMQ ###
Before running any containers for the website start the RabbitMQ container.  Open a command or powershell prompt.

Change to the root directory of where you cloned the repository to.

Run `docker-compose up -d`

You can can access the RabbitMQ Management console at http://localhost:15672

Username: retaildemo

Password: password

To stop RabbitMQ run `docker-compose down`


## Step One ##
Check out the step_one branch which has a working web site but does not publish any events and there are no endpoints yet.

Run the project to launch the store web site at http://localhost:32773/


## Step Two ##
This step adds publishing of the PlaceOrder command by the checkout controller and handling of the message by the Sales endpoint.

#### Create the Sales Endpoint ####

From the `\src` directory run the following commands to create a Sales.Endpoints project using the NServiceBus Docker Container template: 

    mkdir Sales.Endpoints    
    cd Sales.Endpoints
	dotnet new nsbdockercontainer

Add the NServiceBus.RabbitMQ package to the Sales.Endpoints project

`Install-Package NServiceBus.RabbitMQ`

In Visual Studio add the created project to the Sales solution folder.
Edit the Hosts.cs file
Change `EndpointName` to `sales`

Edit the EndpointConfiguration setting to match the store-web settings

Add a `Sales.Endpoints.PlaceOrderHandler` to handle the `PlaceOrder` command

Add a `shipping` service to the `/src/docker-compose.yaml` file


 
## Step Three ##
This steps adds publishing of the OrderPlaced event in the PlaceOrderHandler and handling it using the Billing endpoint.

From the `\src` directory run the following commands to create a Billing.Endpoints project using the NServiceBus Docker Container template: 

    mkdir Billing.Endpoints    
    cd Billing.Endpoints
	dotnet new nsbdockercontainer

Add the NServiceBus.RabbitMQ package to the Billing.Endpoints project

`Install-Package NServiceBus.RabbitMQ`

In Visual Studio add the created project to the Billing solution folder.
Edit the Hosts.cs file
Change `EndpointName` to `billing`

Edit the EndpointConfiguration settings to use Infrastructure.Endpoint.

Create an `OrderPlaced` event in `Sales.Messages.Events`

Publish the `OrderPlaced` event from the `Sales.Endpoints.PlaceOrderHandler` handler

Add a `Billing.Endpoints.OrderPlacedHandler` to handle the `OrderPlaced` event

Add a `billing` service to the `/src/docker-compose.yaml` file


## Step Four ##
This steps adds handling of the OrderPlaced event using the Shipping endpoint.

From the `\src` directory run the following commands to create a Shipping.Endpoints project using the NServiceBus Docker Container template: 

    mkdir Shipping.Endpoints    
    cd Shipping.Endpoints
	dotnet new nsbdockercontainer

Add the NServiceBus.RabbitMQ package to the Billing.Endpoints project

`Install-Package NServiceBus.RabbitMQ`

In Visual Studio add the created project to the Billing solution folder.
Edit the Hosts.cs file
Change `EndpointName` to `shipping`
Edit the EndpointConfiguration settings to use Infrastructure.Endpoint.

Add a `Shipping.Endpoints.OrderPlacedHandler` to handle the `OrderPlaced` event

Add a `shipping` service to the `/src/docker-compose.yaml` file

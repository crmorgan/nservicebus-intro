# nservicebus-intro
Introduction to NServiceBus retail demo

### URLs ###
Store Website: http://localhost:32773/

RabbitMQ Management: http://localhost:15672

Username: retaildemo

Password: password

## Step One ##
Install the NServiceBus .net templates

run `dotnet new -i "ParticularTemplates::*"`


## Step Two ##
from `src\Sales.Endpoints`

run `dotnet new nsbdockercontainer`
 
 
## Step Three ##
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

Publish the `OrderPlaced` event from the `PlaceOrderHandler` handler

Add a `Billing.Endpoints.OrderPlacedHandler` to handle the `OrderPlaced` event

Add a `billing` service to the `/src/docker-compose.yaml` file


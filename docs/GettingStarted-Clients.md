# Working with Clients and Consumers

A _Consumer_ is an actor that uses the functionality of a service.  Generally when we talk about Consumers, we are talking about actors that call the Service API.  A _Client_ is the mechanism by which this calling takes place.  

In that sense, any Service that calls another Service is acting in the capacity of a Consumer.  But for this discussion, we want to draw a bit more of a distinction:  A Consumer is an actor in the Fabric that does not provide any Service functionality but has dependencies on Services (calls Services).  

Taking this restricted sense, a Consumer can be contrasted with a Host.  A host has services that run within it, and the host dependencies as an aggregate list of the dependencies of all of it's Services.  The Host provides an environment in which those Services can run.  The environment must include a number of elements in order for the running Services to function.  

A Consumer on the other hand also provides an environment, but a much simpler one.  The only things that depend on that environment are Clients which are comparatively simple objects.  

What kinds of things might be Consumers?  A UI that calls Services directly would be a Consumer for example.  But a much more common case would be an API Gateway that provides a REST or GraphQL API to a external code.  A web based interface would typically talk to the API Gateway, which would translate those API calls into Service Calls.  

## Using the Consumer helper

The consumer helper does all the init necessary to register and make a service call.  Here is an example of code to do just that: 

```
using XKit.Lib.Consumer;
using Samples.SampleService.V1.Client;
using Samples.SampleService.V1;
using XKit.Lib.Common.Fabric;
using Samples.SampleService.V1.ServiceApiEntities;

var helper = new ConsumerHelper();

helper.CreateInitConsumer(
    dependencies: new[] { Constants.ServiceDescriptor }
);

ISampleServiceClient sampleClient = new SampleServiceClient(
    helper.Log,
    helper.Connector,
    ServiceCallTypeParameters.SyncResult()
);

var result = await sampleClient.SampleCall(new SampleRequest());

Console.WriteLine(result.HasError ? "Failed" : "Succeeded");

```

Basically just construct a `ConsumerHelper` and call `CreateInitConsumer`. 

See the [consumer sample](../samples/SampleConsumer/) for the full project. 

## What about just calling services directly? 

Going through the tutorials, in the very first one we demonstrate bringing up a service can calling the Service API with `curl`.  What is the difference between doing that and constructing a Consumer environment and using Clients?  

If you already know the address of Host and you are aware of how to send requests directly over whatever protocol it uses, you can certainly do that.  However you have to know the address and you have to be aware of how to communicate over the protocol.  By using a Client, all of that is hidden.  The ConsumerHelper registers with a Registry service and gets a list of where the Services are that it is interested in.  The only thing you must provide is the Registry address and after that the rest is handled for you. 

## What about non c# Consumers? 

At the moment of writing this, the XerviceKit libraries are only in c#.  More platforms will be added later. If you want to talk directly to a service using some other stack, you would have to send requests directly to the services.  

However, assuming that there is an API gateway and that it too is on .NET, most other actors will be communicating through that API gateway which is likely to be REST or GraphQL.

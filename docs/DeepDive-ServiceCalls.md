# Service calls

Exactly how service calls work and service call routing is both an important topic and one that is fairly involved.  The recipient of that call might be right next to the caller on the same machine, or it could be located on some remote process.  XerviceKit hides complexity from both the side of the client and that of the server. 

## Defining service calls

Service calls are the way that actors communicate with services.  "Actors" in this case could be consumers, other services, or even code that does not use XerviceKit.  Service calls generally tell the service to do something, request information, or inform the service that something has happened.

Service calls most obviously are part of calling the service api, but in fact are also used for dispatching Events and Commands.  

### Service api

Most services define an "API" which is a set of methods that describe things the service can be asked to do.  The API is described an interface.  Here is an example: 

```
public interface IUserApi : IServiceApi {

    Task<ServiceCallResult<UpsertUsersResponse>> UpsertUsers(UpsertUsersRequest request);

    Task<ServiceCallResult> DeleteUsers(DeleteUsersRequest request);
    
    Task<ServiceCallResult<GetUsersResponse>> GetUsers(GetUsersRequest request);

    Task<ServiceCallResult<GetUserCountResponse>> GetUserCount();   
}
```

A few things to notice:
* The interface by convention has the suffix of `...Api`.
* The interface derives from `IServiceApi`
* An api method can optionally take a "request" object which is just a POCO.  
* An api method always returns a `ServiceCallResult`.  The `ServiceCallResult` can be generic, which means it has no response body, or a response body type can be provided.  If the service call returns data to the caller, it is passed by way of the response body. 

### Events 

When services want to inform any interested actor that something has happened, they use Events.  Events are dispatched by way of service calls.

```
public interface UserEvents : IServiceEvents {

    Task<ServiceCallResult> UsersInserted(UserEventMessage message) 
        => throw new NotImplementedException(); 
    Task<ServiceCallResult> UsersUpdated(UserEventMessage message) 
        => throw new NotImplementedException(); 
    Task<ServiceCallResult> UsersDeleted(UserEventMessage message) 
        => throw new NotImplementedException(); 
}
```

Notice that this is very similar to an api interface.  The thing to remember is that these methods represent code that will be run on the _subscriber_ side.  More about this in the deep dive on events and commands. 

Some things to notice:
* Event interfaces do not follow the convention of prepending with `I`.  
* Event interfaces by convention have the name of the service that emits them and the suffix of `Events`
* Event interfaces derive from `IServiceEvents`
* Methods take a "message" parameter (equivalent to a "request") and return a plain `ServiceCallResult` that does not have a response body. 
* Default implementations are provided.  The reason for this will be more clear in the deep dive on Events and Commands, but the gist is that by doing this, a subscriber is not forced to implement methods that it may not be subscribing to, simply to fulfill the interface.


```
public interface UserCommands : IServiceCommands {
    Task<ServiceCallResult> CreateUserProfile(CreateCommand command) 
        => throw new System.NotImplementedException();
    Task<ServiceCallResult> UpdateUserProfile(UpdateCommand command) 
        => throw new System.NotImplementedException();
}
```

### Commands 

Commands are very similar to Events.  They inform any responsible actors that something needs to be done. 

* Command interfaces do not follow the convention of prepending with `I`.  
* Command interfaces by convention have a descriptive name and the suffix of `Commands`
* Event interfaces derive from `IServiceCommands`
* Methods take a "message" parameter (equivalent to a "request") and return a plain `ServiceCallResult` that does not have a response body. 
* Default implementations are provided.  The reason for this will be more clear in the deep dive on Events and Commands, but the gist is that by doing this, a subscriber is not forced to implement methods that it may not be subscribing to, simply to fulfill the interface.

## Service call Lifecycle

A service call starts on the side of the "Consumer" (the code that is consuming the service) by way of a Client object that translates the call into a request that XerviceKit can handle.  This is the typical workflow.

For the sake of simplicity, we will assume that the protocol being used in this case is the HTTP protocol, which uses HTTP to communicate the service call between the different components here. 

1) The consumer calls a method on the Client

```
var getUsersResult = await client.GetUsers(new GetUsersRequest());
```

2) The Client creates the request object and passes it to an XerviceKit class called a FabricConnector which is responsible for connectivity to the Services in the Fabric.  

3) The FabricConnector has a list of ServiceCallRouters that correspond to individual services.  It hands off the request to the appropriate ServiceCallRouter (also called at times simply a "Call Router")

4) The ServiceCallRouter has a list of instances of the target service.  It selects one of them to pass over the request.  At the same time, it has a list of protocols available to pass over the request.  It also picks the appropriate protocol and then uses it to send the request.  For this example, we are using the HTTP protocol.

5) The HTTP protocol serializes the request and makes a POST to the endpoint that corresponds to the target service. 

By way of the communication protocol chosen, the request arrives to the side of the Host where the Service instance resides.  A communication protocol has both a client side component and a host side component.  The host side component marshals the request into something that the host can understand.  From this point, the workflow continues in the host process and in the service code. 

6) The HTTP protocol receives the request and deserializes it.  It figures out what service is being called and obtains a reference to the service API interface.  Then it simply calls the corresponding service API method with the deserialized request body if there is one. 

7) The service API method creates the _Operation_ object that has the code necessary to do the work involved in fulfilling the call.  

8) The _Operation_ optionally validates the request.  If the request is invalid, it signals to return immediately with an error indicating a bad request.  Otherwise it proceeds.

9) If the caller indicated that they are not interested in the results of the operation ("fire and forget"), the service call returns at this point with a "pending" response.   

10) The main code of the operation is executed, and produces either a success or an error.  

11) Assuming that the caller has indicated they are interested in the results, the protocol packages these as needed (serializing the result for example) and returns the results to the client.  In the case of the HTTP protocol, the method returns with the result object which is serialized by ASP.NET.

At this point, the action switches back to the client side!

12) The instance client (part of the protocol) which made the is particular call to the service returns with a result.  

13) The result bubbles up back to the caller, which receives a ServiceCallResult object that optionally contains the Response Body.  

```
var getUsersResult = await client.GetUsers(new GetUsersRequest());
var theUsers = getUsersResult.ResponseBody.Users;

```

It is important to note that although all of that was happening behind the scenes, the caller simply sees a single method call. 


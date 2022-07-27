# Working with Events and Commands

Events and Commands are way of allowing Services to interact without creating a specific _dependency_ between them.  The forthcoming "deep dive" on this subject will explore more why this is important in some cases.  

## Events

_Events_ allow services to "subscribe" to receive messages when something interesting happens.  They are a way of allowing services to do things based on what other services do.  

### Defining Events

The Service that owns the Events defines them by creating an interface in it's Common library:

```
public interface UserEvents : IServiceEvents {

    Task<ServiceCallResult> UsersInserted(UserEventMessage message) => throw new NotImplementedException();  
}
```

Some things to notice:
* Event interfaces do not follow the convention of prepending with `I`.  
* Event interfaces by convention have the name of the service that emits them and the suffix of `Events`
* Event interfaces derive from `IServiceEvents`
* Methods take a "message" parameter (equivalent to a "request") and return a plain `ServiceCallResult` that does not have a response body. 
* Default implementations are provided.  The reason for is that a subscriber is not forced to implement methods that it may not be subscribing to, simply to fulfill the interface.

### Raising Events

Remember is that the Events defined in the Event interface methods represent code that will be run on the _subscriber_ side.  We "raise" an Event to _signal_ that code elsewhere (on the subscriber) needs to get run. 

In order to raise an Event, an Operation needs a Event Messenger Client.  This is typically created early in the life cycle of the Operation, perhaps in `InitServiceOperation()`

```
protected override Task<bool> InitServiceOperation() {
    try {
        eventMessenger = ClientFactory.Factory.CreateEventMessengerClient<UserEvents>(
            Log,
            HostEnvironment.Connector
        );
        return Task.FromResult(true);
    } catch (Exception ex) {
        Log.Fatality(ex.Message);
        return Task.FromResult(false);
    }
}
```

The Event Messenger Client provides some methods to allow the Service to raise the Event at the right time.   

```
await EventMessenger.RaiseEvent(
    nameof(UserEvents.UsersInserted),
    new UserEventMessage {
        Users = inserted
    }
);
```

### Subscribing to Events

When a service wants to subscribe to an Event, there are three things it must do.  

1) It must include a reference to the Common project of the service with the Event, either directly or by package.  This gives it the Events interface of that service and any message entities. 

2) It's Operation must implement the Event interface of the other service, or at least the particular ones that it will be subscribing to:

```
public class ListeningServiceOperation 
    : ServiceOperation<IListeningServiceOperation>, UserEvents {

    public ListeningServiceOperation(
        ServiceOperationContext context
    ) : base(context) { }

    Task<ServiceCallResult> UserEvents.UsersInserted(
        UserEventMessage message
    ) {
        return RunServiceCall(
            message,
            operationAction: (m) => {
                // ... Do something based on the message
            }
        );
    }
...
```

Notice that this is extremely similar to the way a Service API method is implemented!  In fact, Events are handled by exactly the same mechanisms. 

3) The subscribing Service must register a Subscription to that Event. 

```
public class ListeningService : ManagedService<ListeningServiceOperation> {

    public ListeningService(
        IXKitHostEnvironment localEnv
    ) : base(localEnv) { 
        AddEventSubscription<UserEvents>(nameof(UserEvents.UsersInserted));
    }
...
```

### Receiving Events

If the previously indicated steps are done correctly, then automatically when the emitting Service raises an Event, the code on the Subscriber side will execute. 

## Commands

_Commands_ allow a service to respond to something that needs to be done without the issuer of the Command knowing exactly what Service will respond.  Services listen for _Commands_ they are interested in handling, and the do some work in response. 

### Defining Commands

Unlike Events, Commands may not be owned by a Service.  The associated interface might simply be in a common assembly somewhere.  

```
public interface VideoProcessingCommands : IServiceCommands {

    Task<ServiceCallResult> ConvertVideo(ConvertVideoMessage message) => throw new NotImplementedException();  
}
```

Some things to notice:
* Command interfaces do not follow the convention of prepending with `I`.  
* Command interfaces by convention have a descriptive name and the suffix of `Commands`
* Command interfaces derive from `IServiceCommands`
* Methods take a "message" parameter (equivalent to a "request") and return a plain `ServiceCallResult` that does not have a response body. 
* Default implementations are provided.  The reason for is that a subscriber is not forced to implement methods that it may not be subscribing to, simply to fulfill the interface.

### Issuing Commands

Remember is that the Commands defined in the Command interface methods represent code that will be run on the _subscriber_ side.  We "issue" a Command to _signal_ that code elsewhere (on the subscriber) needs to get run. 

Commands are issues by way of a Command Messenger Client.  

```
messenger = ClientFactory.Factory.CreateCommandMessengerClient<VideoProcessingCommands>(
    Log,
    HostEnvironment.Connector
);
```

The Command Messenger Client provides some methods to allow the caller to issue the command.   

```
messenger.IssueCommand<Message>(
    nameof(VideoProcessingCommands.ConvertVideo), 
    new ConvertVideoMessage { /* ... */ }
);
```

### Subscribing to Commands

When a service wants to subscribe to an Event, there are three things it must do.  

1) It must include a reference assembly with the Command interface. 

2) It's Operation must implement the Command interface, or at least the particular Commands that it will be subscribing to:

```
public class HandlingServiceOperation 
    : ServiceOperation<IHandlingServiceOperation>, VideoProcessingCommands {

    public HandlingServiceOperation(
        ServiceOperationContext context
    ) : base(context) { }

    Task<ServiceCallResult> VideoProcessingCommands.ConvertVideo(
        ConvertVideoMessage message
    ) {
        return RunServiceCall(
            message,
            operationAction: (m) => {
                // ... Do something based on the message
            }
        );
    }
...
```

Like Events, this is extremely similar to the way a Service API method is implemented.  They are all handled by exactly the same mechanisms. 

3) The subscribing Service must register a Subscription to that Command. 

```
public class HandlingService : ManagedService<HandlingServiceOperation> {

    public HandlingService(
        IXKitHostEnvironment localEnv
    ) : base(localEnv) { 
        AddEventSubscription<VideoProcessingCommands>(nameof(VideoProcessingCommands.ConvertVideo));
    }
...
```

### Receiving Commands

If the previously indicated steps are done correctly, then automatically when the Command is issued, the code on the Subscriber side will execute. 

## Testing Events and Commands

Testing Events and Commands is really a two part topic:  The side of the emitter and the side of the subscriber.  In the Service that owns the event we might want to test to see if an Event is raised at the moment and in the conditions that it should be.  On the other hand, the code that handles and Event might need to be tested to see if it really does what we think it should. 

### Testing on the emitter side (source)

The testing library provided by XerviceKit (`XKit.Lib.Testing`) has a test version of the Message Broker service that exposes functionality to help with testing on the emitter side of a message.  Testing is a matter of "intercepting" the message and examining it.  

For example, let's suppose that the User service should raise an Event when new users are created. 

```
if (inserted.Any()) {
    await EventMessenger.RaiseEvent(
        nameof(UserEvents.UsersInserted),
        new UserEventMessage {
            Users = inserted
        }
    );
}
```

The test for that service method would want to set up conditions whereby users get created, and then try to intercept the Event message.  

The test helper automatically includes the test Message Broker by default.  We can intercept the message by use of the `InterceptMessage` method.   

```
UserEventMessage message = null;
testHelper.TestMessageBrokerService.InterceptMessage<UserEvents>(
    nameof(UserEvents.UsersInserted),
    (fabricMessage, results) => {
        message = fabricMessage.JsonPayload.FromJson<UserEventMessage>();
    }
);
```

Then after the actions have been completed, we can check for the message: 

```
message.Users.Should().BeEquivalentTo(
    expectedUsersArray, 
    opt => opt.WithoutStrictOrdering()
);
```

Note that this approach works with both Events and Commands that are emitted by Services.  Of course, if the Command is emitted by something else, the test would depend on how that code is written. 

### Testing on the subscriber side (destination)

Because the same mechanisms as API calls are used for calling Subscribers in response to an Event or Command, to test the Subscriber handling of either is simply a matter of "emitting" the message. 

For example, suppose that some service has subscribed to the `UserEvents.UsersInserted`.  

```
public class ListeningServiceOperation 
    : ServiceOperation<IListeningServiceOperation>, UserEvents {

...
    Task<ServiceCallResult> UserEvents.UsersInserted(
        UserEventMessage message
    ) {
        return RunServiceCall(
            message,
            operationAction: (m) => {
                // ... Do something based on the message
            }
        );
    }
...
```

In order to test the code, the test simply has to emit the message (in this case, raise the Event).

```
await testHelper.TestMessageBrokerService.SendMessage(
    nameof(UserEvents.UsersInserted),
    new UserEventMessage {
        ...
    }
);
```

After sending the message (Event or Command), the test would simply check the expectations. 

# Working with Daemons

Daemons allow work to be done "in the background".  A daemon in XerviceKit processes in one of two contexts:  A "timer" event that runs at some point based on a delay, and a "message queue" of things that need to be processed.  See the deep dive on daemons (TBD) for more information as to why things are done this way. 

## Creating a daemon 

### The Daemon class implementation

A Daemon is a separate object from the Service that it is associated with.  Typically you start with creating the Daemon class itself.  

```
public interface IDoSomethingDaemon : IServiceDaemon<DaemonMessage> { } 

public class DoSomethingDaemon : ServiceDaemon<DoSomethingDaemonOperation, DaemonMessage>, IDoSomethingDaemon 
{
    protected override string Name => "DoSomethingDaemon";

    public DoSomethingDaemon() 
        : base(XKit.Lib.Log.LogSessionFactory.Factory) 
    {
    }
}
```

Just like a Service, a Daemon can implement an interface if it provides methods to be consumed by it's Operations, (`IDoSomethingDaemon` in the above example).  The Daemon class that we derive can override different methods in order to customize how it works.  We'll talk more about that later.  

The Daemon needs to be associated with the Service by calling `AddDaemon()` in the Service constructor: 

```
public class ServiceWithDaemon : ManagedService<ServiceWithDaemonOperation> 
{
    public SvcWithAutoMessagingService(IXKitHostEnvironment hostEnv) : base(hostEnv) 
    { 
        AddDaemon(new DoSomethingDaemon());  // <---
    }
}
```


## Daemon operations

A Daemon must have an Operation, just like a Service, in order to do work.  

```
public class DoSomethingDaemonOperation : ServiceDaemonOperation<DaemonMessage> 
{
    public DoSomethingDaemon(ServiceDaemonOperationContext context) : base(context) 
    { 
    }
}
```

The difference is that an Operation for a Daemon does not implement an API interface.  Rather, the Operation class will override methods depending on the kind of work to be done, either by the Timer or Message Queue.

## Timer 

Daemons have a built in timer that can be made to fire at some given point in the future.  This allows the Daemon to do periodic work, such as checking for certain conditions.  

In order to enable the timer, three things must be done:

1) The timer must be enabled in the Daemon itself. 
2) The Operation must override the method `DoTimerOperation()`
3) The Daemon must somehow indicate when the timer should fire. 

#### Enabling the timer

The timer must be enabled in the Daemon itself.  This is typically done in the constructor.

```
public class DoSomethingDaemon : ServiceDaemon<DoSomethingDaemonOperation, DaemonMessage>, IDoSomethingDaemon 
{
    protected override string Name => "DoSomethingDaemon";

    public DoSomethingDaemon() : base(XKit.Lib.Log.LogSessionFactory.Factory) 
    {
        EnableTimerEvent = true;  // <---
    }
}
```

#### Overriding the method `DoTimerOperation()`

This is very simple.  In the Daemon's operation, override `DoTimerOperation()` and add the code you want to run.

```
public class DoSomethingDaemonOperation : ServiceDaemonOperation<DaemonMessage> 
{

    public DoSomethingDaemon(ServiceDaemonOperationContext context) 
        : base(context) { }

    protected override Task DoTimerOperation()   // <---
    {
        // ... Do some work ...
        return Task.CompletedTask;
    }
}
```

#### Determining when the timer should fire

The timer is very flexible about when it fires.  It is very easy to implement a timer that fires on at a regular interval, or one that fires at specific times.  

There are two ways to control this:  

1) If it is always possible to calculate when the timer should fire, override `OnDetermineTimerEventPeriod()` in the Daemon itself and have it return the millisecond delay that should be used. 

```
public class DoSomethingDaemon : ServiceDaemon<DoSomethingDaemonOperation, DaemonMessage>, IDoSomethingDaemon 
{
    protected override string Name => "DoSomethingDaemon";

    public DoSomethingDaemon() : base(XKit.Lib.Log.LogSessionFactory.Factory) 
    {
        EnableTimerEvent = true;  
    }

    protected override uint? OnDetermineTimerEventPeriod() => 900;  // <---
}
```

`OnDetermineTimerEventPeriod()` is called when the daemon is changing run state, for example when it is starting or resuming, as well as directly after each time the timer fires.  In the above example, the timer will fire every 900 milliseconds always.  But this could have easily been a number based on some calculation.  For example, if you wanted the timer to fire based on some event in a calendar, you would calculate how far in the future the event is and then return an appropriate interval for each "next" calendar event. 

If `OnDetermineTimerEventPeriod()` returns `null`, then the timer will not fire again until `SetTimerDelay()` is called (as indicated below) or the daemon changes run states again.  If `OnDetermineTimerEventPeriod()` returns `0` then the timer fires immediately. 

Before of the fact that `OnDetermineTimerEventPeriod()` does not run in the context of an Operation.  The code that it runs should be quick and simple.  Otherwise, use option number 2.   Also, if `OnDetermineTimerEventPeriod()` is not overridden, then the timer will always fire when the daemon is changing state, such as when starting or resuming. 

2) If it is not possible to calculate ahead of time when the timer should fire, if those calculation are extensive and should be run the context of an Operation, then call `SetTimerDelay()`.  This would typically be done within an Operation, possibly even when the timer itself has fired.

```
public class DoSomethingDaemonOperation : ServiceDaemonOperation<DaemonMessage> 
{

    public DoSomethingDaemon(ServiceDaemonOperationContext context) 
        : base(context) { }

    protected override Task DoTimerOperation()   // <---
    {
        // ... Do some work ...

        var delay = /* calculate some value */
        Daemon.SetTimerDelay(delay);
        return Task.CompletedTask;
    }
}
```

A few observations... If you use `SetTimerDelay()`
* Then do not override `OnDetermineTimerEventPeriod()` or else have it always return `null`.  Trying to use both will create a truly complicated situation that will be difficult to understand and debug.
* If `OnDetermineTimerEventPeriod()` is not overridden, then the timer will always fire when the daemon is changing state, such as when starting or resuming. 

## Message Queue

Daemons are designed to do work "in the background", and one way to do this besides use of the timer is by way of the message queue.  The message queue is a basically a list of waiting work items that the daemon can work on.  This work is done asynchronously on background threads.  You an control how many background threads can run at a time.  

The message queue is really about two things, "posting" messages and "processing" them.

#### Defining message objects

The objects in the message queue contain the information that is needed when the daemon Operation goes to process.  They are simple POCO's (Plain Old Clr Objects) just like Requests and Responses in the Service API.

#### Adding messages (posting)

Add a message by calling `PostMessage()` on the Daemon.

```
daemon.PostMessage(new DaemonMessage { /* ... */ });
```

`PostMessage()` will put the message object into the list of "waiting work".  Note that it has a parameter `bool triggerProcessing` which defaults to `true`.  See the next section for more information. 

#### Processing messages

Messages are processed (normally) in the background on worker threads.  Exactly when this happens can be controlled. 

If `triggerProcessing` is `true` when calling `PostMessage()` or `PostMessages()`, then message processing in the background will be started if it was not already in progress.  However if `triggerProcessing` is `false`, then processing will not happen until a call to `ProcessMessages()`.  

`ProcessMessages()` has a parameter `background` that defaults to `true`.  When `true`, worker threads will be created an d messages will process on those worker threads.  If for some reason the caller wanted messages to process synchronously on the current thread, `background` could be set to `false`, but normally this would not be done. 

The code to do the processing is placed in an override of `DoMessageOperation()` on the Operation. 

```
public class DoSomethingDaemonOperation : ServiceDaemonOperation<DaemonMessage> 
{

    public DoSomethingDaemon(ServiceDaemonOperationContext context) 
        : base(context) { }

    protected override async Task<OperationResult> DoMessageOperation(DaemonMessage message) {
        /* ... do something amazing with the message */
        return ResultSuccess();
    }
}
```

#### Controlling the number of background threads

The maximum number of background threads for processing messages is set with an internal property on the Daemon, `MaxConcurrentMessages`.  

## How the Timer and Message Queue work together

Although they might appear to not really be related, the timer and message processing are designed to work together.  The timer might have code to check conditions and then post messages to be handled in the background for example. 

```
public class DoSomethingDaemonOperation : ServiceDaemonOperation<DaemonMessage> 
{
    public DoSomethingDaemon(ServiceDaemonOperationContext context) 
        : base(context) { }

    protected override uint? OnDetermineTimerEventPeriod() => 2000;

    protected override Task DoTimerOperation()   // <---
    {
        // ... Check if we need to add messages ...

        Daemon.PostMessage(new DaemonMessage { /* fill in message */ }, triggerProcessing: true);
        return Task.CompletedTask;
    }
    
    protected override async Task<OperationResult> DoMessageOperation(DaemonMessage message) {
        /* ... do something amazing with the message */
        return ResultSuccess();
    }
}
```

## Testing

Because of the background processing that happens with Daemons, testing could end up being quite complex.  Thankfully, Daemons have a "Debug mode" that intends to simplify things by forcing everything to happen synchronously.  

To use debug mode, in the beginning of a test, before taking any actions, call `SetDebugMode` on the Daemon. You will have to add some code to get a reference to the daemon first.  For example:

```
var daemon = (IServiceDaemon) testHelper
    .Host
    .GetManagedService(Constants.ServiceDescriptor)
    .GetDaemon(nameof(DoSomethingDaemon));
daemon.SetDebugMode(true);
```

This is likely something you want to do as part of the test init, perhaps for all tests.  Or you might have other ways of conveniently getting the reference, perhaps exposing it via a property on your Service.

Once debug mode is set, you can manually control when the timer fires or a message processes, using `DebugFireTimerEvent()` and `DebugProcessOneMessage()`.

```
// ... set up come conditions...
daemon.DebugFireTimerEvent();
// ... check to see if the expectation are correct after the timer fires
daemon.DebugProcessOneMessage();
// ... check to see if the expectation are correct after the message processes
```


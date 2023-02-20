# Basic concepts

## Fabric

XerviceKit has the concept of a _Fabric_, that is, a unified set of actors that together form the backend (everything except the user interface) of an application.  

## Services

XerviceKit is organized around the concept of _Services_.  Services provide some discreet functionality to an application.  For example, a "user" service might be responsible for storing and retrieving information about users.  A "session" service (or perhaps an "authentication" service) might be responsible for managing user logins and logouts.  Etc.  Services ideally should be "single responsibility".  

## Hosts

_Services_ need an environment in which they run.  The environment in which they run provides various things that all _Services_ will need, such as protocols to talk to other _Services_, common resources such as database client factories, and other such items.  The entity that provides this environment is the _Host_.  The _Host_ will load _Services_, tell them when to pause, resume, or stop, provide for commonly used resources, and so forth.  

_Hosts_ are intelligent, and their functionality can be extended by way of _Meta Services_.  A _Host_ can run any number of services depending on how the application is set up.  Usually, there is just one _Host_ per OS process.  Hosts are the primary actors in the _Fabric_. 

Note that a _Host_ is agnostic about the details of any service it runs.  That is, any _Host_ can run any _Service_ and any _Service_ can run in any host.  A service is likewise agnostic about it's _Host_.  These two conceptual concepts interact with each only through general interfaces and so are loosely coupled.  

For more information about _Hosts_ work and what you can do with them, take a look at [the deep dive on the subject of hosts](DeepDive-Hosts.md).

## Managed Services

Most services provide functionality for the application of which they form a part.  These services are _Managed Services_ because they are in a sense managed by the XerviceKit framework.  Unless otherwise specified in this documentation, when referring to _Services_ we are generally talking about _managed services_. 

## Meta Services

Even though most services provide functionality to the application (_Managed Services_), certain special services provide capabilities to the _Host_ itself where they run.  These services are responsible for the aspects of the way pieces of the system interact.  For example, a _Meta Service_ might be responsible for updating configuration information for the _Host_.  Or it _Meta Service_ might be responsible for exposing access to the local log, etc.  

_Meta Services_ are an advanced topic that you do not need to understand deeply unless you are planning on extending _Host_ functionality. 

## Platform services

There is a special category of _Service_ are _platform services_.  _Platform services_ are _Managed Services_ just like any other with the exception that they provide functionality that the "platform" (the overall application environment) needs to run.  _Platform services_ strictly provide functionality for other _Services_ to operate or to enable certain XerviceKit features.  For example, if a service is proving configuration information to other services, that would be a "platform" function.  The Registry Service (that keeps track of which _Hosts_ are providing which _Services_ for the purpose of coordinating _Service calls_) is an example of a _Platform Service_. 

One trait of _Platform Services_ is that they are application agnostic.  The same _Platform Service_ works the same way regardless of the application in which it runs.  

## Other service categories and how services identify themselves

_Platform services_ are the only category of service that the XerviceKit framework is specifically aware of.  Depending on the architecture of the application, there might be other categories of services however. A Service _Category_ is a lightweight concept that just serves to organize _Services_.  For example, if you have _Services_ that allow the application to manage it's own infrastructure, those might be categorized as "Infrastructure Services".  _Services_ that manage access to external resources might be "Resource Services".  Etc.  

The only point here that perhaps stands out is that categories might be roughly aligned with service _Collections_.  A _Service_ identifies itself with three pieces of information:  Collection, Name, and Version.  

"Collection" is simply a general grouping of like services.  
"Name" is the name of the service itself, which should describe what functionality it provides.  
"Version" is the major version of the service.  Any breaking changes in the way a service works should be accompanied by a change in the Version of the service.  

## Service calls through the service api

One of the most common types of functionality that a service provides is an API through which _Service calls_ can be made.  Think of a _Service call_ as you would a typical method call, except for the fact that the execution is actually happening "remotely" within the context of the service.  "Remotely" in this case simply means "somewhere else"; where exactly that happens to be is irrelevant.  It might actually be happening in the same process (on the same _Host_), or in a container somewhere on the machine, or on a different server.  XerviceKit takes care of figuring out where the service is actually located and how to convey the call and return with the results.  A service defines what calls are available by defining a simple c# interface and the framework does the rest. 

For more information about how _Service calls_ work, including a step by step of the workflow, take a look at [the deep dive on the subject of service calls](DeepDive-ServiceCalls.md).

## Dependencies

Of course, a service might have a _dependency_ on another service in order to do it's job.  The "session" service mentioned above would need to know who the users were, perhaps needing access specifically to the list of user names and a hash of their passwords in order to authenticate them and start a session.  So one service can make use of the functionality provided by another one, that is, it's _dependency_.  

It is important to understand that when a _Service_ has a _dependency_, it not aware of details of how that _dependency_ does it's job.  It simply has access to an interface that describes the functionality and it knows the Collection, Name, and Version (together called the "Descriptor") of the _dependency_.  Why is this important?  Because it means that _dependencies_ can easily be swapped.  It also means that services can more easily be tested, because a mock service simply has to implement the same interface and have the same Descriptor.

For more information about how _Service calls_ work, including a step by step of the workflow, take a look at [the deep dive on the subject of service calls](DeepDive-ServiceCalls.md).

## Operations

It is important to understand how a service conceptually deals with the concept of "statelessness".  Ideally a service should not keep any state information.  It should essentially be "stateless".  The reason that keeping state is generally considered bad practice is that it introduces potential for bugs and limits scalability.  However, there are certain concepts around services that are essentially stateful.  For example, a service is either running (active state) or not (inactive state).  Additionally, there might be certain expensive resources that must live over the lifetime of the service because to create and destroy them each time a service call happens would not be feasible.  

To solve this, XerviceKit makes a distinction between the main service code that lives and dies with the host, and the transitory code that executes only when the service is "doing something".  This later code is confined to an _Operation_.  An _Operation_ is an object that gets created when the service is about to do a thing, it executes code to do the thing, and then is destroyed.  Any state information that changes during the execution of the operation is destroyed by the end of the operation.  

_Service calls_, _Events_, _Commands_, and _Daemons_ all make use of _Operations_ to isolate the "doing stuff" part of execution. 

## Events and Commands

Services can provide _Events_ which allow other services to "subscribe" to receive messages when something interesting happens.  Events are a way of allowing services to do things based on what other services do, but without creating a specific dependency between them.  

_Commands_ are similar to _Events_ but in reverse.  A _Command_ allows some actor in the application to say "Do a thing!" without actually knowing which service or services will do the thing.  In this case, services indicate what _Commands_ they are interested in handling, and then when the command is issued they are informed.  When receiving notification of the command, they can do some work in response. 

For more information about _Events_ and _Commands_, take a look at [the deep dive on the subject](DeepDive-EventsAndCommands.md).

## Daemons

At times you need something to happen periodically, or in the background in response to something.  _Daemons_ provide these two abilities which go hand in hand.  

A _Daemon_ can have a timer which periodically triggers an operation.  This allows a service to do things like check for some conditions which might require some work to be done.  

A _Daemon_ also has associated with it a "message queue".  A message queue is a queue of objects that describe something that needs to be done.  The daemon will schedule this work to be done in the background.  

Let's say for example that an application wants to process and convert video files that are dropped into a certain folder.  A daemon might be written which periodically checks the folder based on a timer.  When the operation that checks the folder finds files in it, it could then add messages to the queue, with each message corresponding to one of the files.  The daemon would then schedule the files to be processed in the background.  

As a slightly different scenario, we could imagine that the service where the daemon itself is running is responsible for placing these video files in the folder to be processed.  In that case, the daemon itself would not have to monitor the folder and the timer part would be unnecessary.  When the service places the files in the folder, it itself would simply add a message to the daemon's message queue as part of the operation.  The daemon would then schedule the processing in the background. 

For more information about _Daemons_, take a look at [the deep dive on the subject of hosts](DeepDive-Daemons.md).


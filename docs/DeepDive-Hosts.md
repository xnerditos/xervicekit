# Hosts

In typical service environments, a host is fairly "dumb".  It simply provides a place where a service or services lives.  It is often associated with a particular web server framework, and is essentially the same as the OS process.  Also, quite often a service is tied to and runs in the context of the one host.  So deploying the service and deploying the host are one and the same. 

XerviceKit has a different concept of what comprises a "host".  A host ...

* Are the primary actors in the fabric
* Provides a standardized environment in which services run
* Provides a interface in code for controlling services
* Potentially provides an api for other actors to interface with it.
* Is intelligent, able to interact with the services it runs and potentially with other hosts
* Is extensible and can be given new capabilities
* Provides the protocols for services to communicate with each other
* Carries out the steps needed for a service to made available to the fabric.  Primarily, this means that it handles registration concerns. 

## Quick start

The code below represents a "minimal" example to bring up a host. 

```
HostEnvironmentHelper hostEnvHelper = new();

void Startup() {
    hostEnvHelper.CreateInitHost();
    // Register objects potentially needed by services
    //hostEnvHelper.Host.ObjectRepositoryRegisterSingleton(
    //    RavenDocumentRepositoryFactory.CreateFactory(),
    //    typeof(XKit.Lib.Data.DocumentRepository.IDocumentRepositoryFactory)
    //);
    hostEnvHelper.Host.AddManagedService(
        MyService.V2.MyServiceFactory.Create
    );
    hostEnvHelper.StartHost();
}

void Teardown() {
    hostEnvHelper.Host.StopHost();
}
```


## Lifecycle

### Starting up

#### Pre-initialization

Ahead of initialization, the consuming code may need to set up some other libraries or frameworks.  For example, if the host is communicating over HTTP, then Kestrel will need to be set up.  

#### Initialization

Initialization involves creation of the objects necessary to bring up a host.  All of this is handled for you by a call to `HostEnvironmentHelper` but of course can also be done manually. 

```
HostEnvironmentHelper hostEnvHelper = new();
hostEnvHelper.CreateInitHost();
```

By default `CreateInitHost()` will choose sensible defaults for initializing the host.  It will look at local environment variables as well when deciding options about how to initialize the host.  Parameters can also be passed in to the call to control aspects of initialization.

What are the steps it actually does? 

1) Ensure certain local environmental items are cared for, such as creation of folders
2) Create objects that are needed for creating the host.  This includes the configuration session factory, instance client factories, and the connector. 
* The configuration session factory is responsible for providing access to configuration that services may need. 
* The instance client factories are part of the _communication protocols_ that are used to let services communicate with each other.  The services themselves do not know how their requests and responses are transmitted.  The host (by way of the connector) takes care of all of this.  
* The connector is responsible for understanding how to move requests and responses between services.  It makes use of the instance client factories.
3) Create the host object
4) Load default meta services, which provide the host with additional capabilities

#### Register object dependencies

After calling `CreateInitHost()`, the calling code may have to register objects it knows the services running in the host may need.  For example, objects to access a database.  

Typically, the services themselves consume only interfaces.  The object registration provides the concrete implementations.  

For example, if a service requires a `XKit.Lib.Data.DocumentRepository.IDocumentRepositoryFactory` in order to access the database, the code initializing the host knows what concrete database is going to be used.  It provides it with a call to `ObjectRepositoryRegisterSingleton`

```
hostEnvHelper.Host.ObjectRepositoryRegisterSingleton(
    RavenDocumentRepositoryFactory.CreateFactory(),
    typeof(XKit.Lib.Data.DocumentRepository.IDocumentRepositoryFactory)
);
```

When a service needs a `IDocumentRepositoryFactory`, it will ask the `IXkitHostEnvironment` for it and receive the object that was registered.  

#### Register services

The host will need to know what services it will be running, of course.  This is accomplished with a call to `AddManagedService()`.  

```
hostEnvHelper.Host.AddManagedService(
    MyService.V2.MyServiceFactory.Create
);
```

Of course more than one service can be added.  At this point, if there were any additional service capabilities to add, `AddMetaService()` could be called as well. 

#### Host Startup

```
hostEnvHelper.StartHost();
```

The host can also be started directly with a call to `IXkitHost.StartHost()`

Internally, what happens during the host startup?  Primarily two things:

1) Services are started up
2) The host registers itself with an instance of the Registry service running somewhere.  

If `HostEnvironmentHelper.StartHost()` is used, it will use environment variables to obtain the address to an instance of the Registry service.  If `IXkitHost.StartHost()` is used, then the address to the Registry must be provided.  This single piece of information, the address to an instance of Registry is all that is needed for a host to join the fabric and for it's services to be available as well as to be able to call other services.  The Registry, during the registration process, determines the dependencies of the services running in the host and returns address information that is used in future service calls to dependencies. 

### Tearing down

Stopping a host is very simple:

```
hostEnvHelper.Host.StopHost();
```

During the call to `StopHost()`, each running service is stopped, the host unregisters itself with the Registry service, and resources are freed. 

## Environment variables used by `HostEnvironmentHelper`

`HostEnvironmentHelper` is intended to make host startup and management super easy.  It uses sensible defaults where it can, and use environment variables to determine how to do various aspects of init and startup.  These are the variables that it uses.  If you use `HostEnvironmentHelper`, you can make these env vars standard in your deployment. 

| Environment Variable | Default | Description |
| ---- | ---- | ----
| HOST_BASE_ADDRESS | <none> | Required.  This is used for the host to indicate how it is reachable by other hosts.  As of the writing of this documentation, this should be an ip address (optionally with port) that can be used for web requests. |
| LOCAL_DATA_FOLDER_PATH | `SpecialFolder.ApplicationData() + "/xkit-data"` | The folder where a service could potentially put data it owns. |
| METADATA_DB_PATH | <none> | Not used at the moment. |
| INITIAL_REGISTRY_ADDRESSES | <none> | A semi-colon separated list of addresses where the Registry service can be found.  Only one is needed but multiple are optional. |
| CONFIG_FOLDER | LOCAL_DATA_FOLDER_PATH + "/config" | The folder where service configuration files are be stored. |

## What if there is no Registry service?

Hosts are designed to have a number of features to enable services to "just work".  A host is also designed to "degrade gracefully" if it does not have everything that it needs.  So what happens if there is no Registry service available?  This is a actually a fairly normal use case.  For some applications, they may be simple enough that it does not make sense to run services in separate hosts.  Or perhaps if an application is in "proof of concept" stage, it is simply easier to start off as a monolith.  

Thankfully hosts are smart enough to transparently handle such use cases.  If a Registry is not available, then a host simple carries out the Registry functions internally within it's own environment.  So if service A depends on service B, if they are both running on the same host, the host will figure out how to connect the dependencies.  

This allows for a nice on-ramp during initial development.  When starting to build a new application, services can simply be included in the same host and everything will "just work".  

## What about the other Platform services? 

Certain features of XerviceKit require special service (dubbed "Platform" services) in order to work.  The Registry is the most obvious of these, but there are a few others and more might be added over time.  As in the case of the Registry, if one of these is not present then the XerviceKit framework will simply default to a sensible built-in mechanism.  

For example, in order for Events and Commands to work properly, a service called the Message Broker must be available.  However if the services are all running on the same host anyway, then the framework itself can figure out how to do these functions.  This makes building a monolithic version of the application extremely fast and easy.  Everything just works "out of the box".  

Samples are provided for each of the platform services.  Although these should not be considered "production ready" because of their simplistic implementations, they can be used as as a starting point for building the "real" versions of these services you plan to use. They can also be used as a temporary stand-in until you have your own implementations.   

## Objects

| Object | Description
| ----       | ---   
| XKitHost | The primary implementation of an XKit host.  It implements all three of the primary interfaces. 
| IXKitHost | Interface to control the host.  Consumed by the calling code.  
| IXKitHostEnvironment | Interface to the host that is provided to services.  Since a service can have dependencies, it makes use of both this interface and `IXKitEnvironment`.
| IXKitEnvironment | Interface used by consumers (service clients) to work within their environment.  When the environment is a host, this interface interacts with the host. 
| HostEnvironmentHelper | A helper class that provides methods to quickly and easily bring up a host.   



TODO:  Needs re-write

# How to use Host library

## XKit Host services functionality

Services implemented in the XKit Host framework are agnostic of the transport mechanism.  The library handles the marshalling of the result across across the wire.  The expected status code is defined from a limited selection, thus allowing the library to understand what to do in each case.  

Services consist of two main components: 

1) The service itself, which defines attributes and mechanisms that live for the lifetime of the service instance, such as service description information, etc.  This component also handles stopping and starting activities, and owns any resources that live for the lifetime of the service. 

2) A service operation, which defines attributes and mechanisms that live for the life of a single operation.  This includes any operation specific initialization and resources.  

The library defines requirements around logging, instantiation of the operation, etc.  Services are designed from the ground up on the principle that each operation is atomic, creates a traceable history of it's execution, and has standard entry and exit mechanisms.

An operation describes it's execution by way of IOperationMonitor.  IOperationMonitor provides visibility into the execution of the operation and it's progress.  The default monitor does things like logging, but it in theory could be replaced with a custom monitor if the caller of the operation needed to react to certain conditions during execution.  An operation is expected to integrate calls to the monitor in such a way as to allow traceability or reproduction of the operation.

The default implementaion of IOperationMonitory provides hooks for responding to and handling certain events during operation.  It also provides logging.

## Developing services

Services consist of ... 

1) A "service" interface that defines service.  This represents the service without any of the boilerplate start / stop mechanism, operation specific stuff, etc.  The core provides any service level methods that the oepration will have access to. 
* naming convention follows the pattern of IxxxxService
* derives from IService (or IMetaService) and IServiceBase<IxxxxOperation>
* defines "method services" that the operation will need to carry out its work. 

2) An "operation" interface 
* Follows the naming convention IxxxxOperation
* Inherits IServiceOperation
* Has methods for each of the service operations

3) An implementation of the service itself
* Follows the convention xxxxService
* Implements IxxxxService
* Owns any service lifetime stuff
* Owns any start / stop activities
* Can assume that only one instance will exist within any proces space. 
* Must not make any assumptions about reentrancy of methods corresponding to IxxxxCore or IxxxxService
* Ideally, dervies from ManagedServiceBase which will handle most of the implementation of the interfaces and expose just a few abstract methods to be implemented by the derived class. 

4) An implementation of the service operation itself
* Follows the naming convention xxxxOperation
* Lives for the duration of one operation
* Must not make any assumptions about reentrancy

5) A controller that exposes the service over HTTP
* Follows the naming convention xxxxController
* Ideally, derives from ServiceControlerBase, which provides standard code for instantiation of operations, etc.   

See samples

## Startup

0) Pre-init
* If any of the default behaviour should be overriden, use the corresponding method InjectCustomFactory for that factory to inject your own implementation
* Set a health checker with a call to HostManagerFactory.SetHealthChecker()

1) Init
* Create the IHostManager singleton with a call to HostManagerFactory.CreateSingleton()
* Register additional (non default) meta services that run in the context of the host
* Create service implementation objects and do any init activity that is required
* Register any service implementations with hostManager.RegisterService().  
* Start the host

2) Add controllers to asp.net.  In Startup.cs ...
* Include a call to AddXKitHostManagement() to add the core meta api controllers
* Call AddApplicationType() for controllers of any managed services that are in nuget packages. 

// Example
using XKit.Lib.Host;
...
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddMvcCore()
        .AddXKitHostMvc()
        .AddApplicationType<MyServiceController>();
}




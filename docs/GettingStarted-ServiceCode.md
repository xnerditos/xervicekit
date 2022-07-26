# Defining the service code

If a service provides methods that it's operations (or other callers) can use, you will need to define a service interface and then derive a class. 

## Service Interface

The service interface will look something like this, and should go in the Service project: 

```
using XKit.Lib.Common.Host;

namespace TestServices.SvcWithDependency1;

public interface IServiceName : IManagedService {
    Task<Something> SomeMethodProvidedByTheService();
}
```

The only interesting thing about the service interface is that it should derive from `IManagedService`.  Because the Operation will have access to the service interface, it will be able to call methods such as `SomeMethodProvidedByTheService()`.  There are no restrictions on the signature of such methods. 

## Service class

```
using XKit.Lib.Common.Registration;
using XKit.Lib.Host.DefaultBaseClasses;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Utility;

namespace ServiceName;

public class ServiceNameService : ManagedService<OperationName>, IServiceName 
{
    private static readonly IReadOnlyDescriptor descriptor = Constants.ServiceDescriptor;
    
    protected override IReadOnlyDescriptor Descriptor => descriptor;

    protected override IEnumerable<IReadOnlyDescriptor> Dependencies => new[] 
    {
        SvcDependency.Common.Constants.ServiceDescriptor
    };

    public SvcWithDependency1Service(IXKitHostEnvironment hostEnv) 
        : base(hostEnv) {}

    Task<Something> IServiceName.SomeMethodProvidedByTheService() 
    {
        // TODO:  Do something interesting
    }
}
```

Service classes should:

* Derive from `ManagedService` and provide the type of the operation as a type parameter
* Implement the associated service interface.

[Go back to 'Getting Started with Services'](./GettingStarted-Services.md)

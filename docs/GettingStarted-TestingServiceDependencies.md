# Testing Services with Dependencies

When a service calls another service, your approach to testing will depend on what kind of tests you are doing.  

* _Unit tests_ in this context make assertions about functionality of a service _as a unit_, disconnected from other services and by itself.
* _Integration tests_ make assertions about functionality of a service while it is connected to it's dependencies.

Note that this information is presented with the assumption that the dependency service is being used by one of the Service API methods.  But really, it doesn't matter.  Wherever the dependency is being called from, these approaches will work. 

## Unit tests

Because unit tests try to isolate the service functionality under test, when there are dependencies these have to be _mocked_.  Mocking provides method stubs which "stand in" for the real dependencies and return a contrived response.  

The test helper has some methods to help us to create mocked dependencies.  There are two ways you can stub the methods themselves.  

* You can rely on the widely used library `Moq` to create the stubs.  This is good when the logic of the mocks is simple.  
* You can create entire mocked classes that pretend to be the dependencies.  Let's look at both.

### Mocking with Moq

In order to us Moq, obviously you must be familiar with it.  This guide assumes you already know how to use Moq. 

Assuming your service has a dependency on another service called "TheDependency" and calls the method "SomeMethod" on that service, you can add lines like this to your `ClassInit()` method.  It should be added _after_ `testHelper.InitializeLocalTestHost();` and _before_ `testHelper.StartHost();`:

```
// Add the mocked service
var theDependencyMock = testHelper.AddMockService<ITheDependencyApi>(TheDependency.Constants.ServiceDescriptor);

// Add a mock for a particular method
theDependencyMock.ApiMock
    .Setup(x => x.SomeMethod(It.IsAny<TheDependency.SomeMethodRequest>()))
    .ReturnsAsync(new ServiceCallResult<TheDependency.SomeMethodResult> { 
        OperationStatus = XKit.Lib.Common.Log.LogResultStatusEnum.Success,
        ServiceCallStatus = ServiceCallStatusEnum.Completed,
        ResponseBody = new TheDependency.SomeMethodResult {
            SomeData = "abc"
        }
    });
```

Obviously, you can mock as many methods as you need to.  Any functionality provided by Moq can be used. 

### Mocking with a "real" service

The other approach to mocking a dependency is to simply write a version of the service for the purpose of testing.  

For example, you could add a file called `MockTheDependencyOperation.cs` with the following:

```
using System;
using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Host.DefaultBaseClasses;

namespace SomeService.Test;

public class MockTheDependencyOperation : ServiceOperation, ITheDependencyApi
{
    public MockTheDependencyOperation(ServiceOperationContext context) 
        : base(context)
    {
    }

    public Task<ServiceCallResult<SomeMethodResult>> SomeMethod(SomeMethodRequest request) {
        return RunServiceCall(
            request,
            operationAction: (req) => {
                // TODO:  Mocked method test logic here
            }
        );
    }
}
```

Then add it to the test by adding this line in the `ClassInit()`

```
// Add the mocked service
testHelper.AddCreateService(
    TheDependency.Constants.ServiceDescriptor,
    typeof(MockTheDependencyOperation)
);
```

Why does this work?  Because as long as the mock identifies itself with the Service Descriptor, the framework will find it as the dependency. 

## Integration tests

The approach with dependencies in Integration Tests is very similar to how we would reference dependencies in the actual service code.  There is no mocking in this case.  We just add the "real" service.  

So however you normally add the dependent service to the host, do the same thing in `ClassInit()`.

This, if there is no specialized service code...
```
// Add the mocked service
testHelper.AddCreateService(
    TheDependency.Constants.ServiceDescriptor,
    typeof(TheDependencyOperation)
);
```

or this, if there is specialized service code. 

```
// Add the mocked service
testHelper.AddService(
    new TheDependency(testHelper.Host)
);
```

When the service under test tries to make a call to `TheDependency`, the XerviceKit framework will simply find it. 



# Tutorial 3:  Calling one service from another

[TOC]

## Description and objectives

This tutorial will:

* Introduce you to multi-service projects.  
* Introduce you to the concept of "dependencies" in which one service makes use of (calls) another.
* Introduce you to non-generic services.  The User service only had an ApiOperation.cs because there was nothing special about the service itself.  The Operation had everything it needed.  But in some cases, we need to include a Service class in addition to an Operation class.  This tutorial will show you how to do that. 
* Help you put into practice what you learned before by having you create a service from the start.
* Show you how to unit test service calls by using mocked dependencies. 

## Prerequisites

* You either previously went through Tutorial 2 or else copied the code from the repository.

## Concepts

These are the concepts that you should understand as you begin this tutorial.  Review the [Basic Concepts](../../docs/BasicConcepts.md) for more information.

* Consumer
* Client
* Service
* Host
* Dependency

## Step 1:  Create the Session service

We already have our User service.  Now create a service called Session based on how we created the User service.  Create each of the projects necessary for the Session service.  You can use what you did for User as a model. 

* Tutorial.Session.Common 
* Tutorial.Session (the service itself)
* Tutorial.Session.Client
* Tutorial.Session.Test

### Tutorial.Session.Common

Create the Common project.  Use what you did previously for User.Common as to guide you.  This will be the ISessionApi interface (which will determine the service methods)

``` 
public interface ISessionApi : IServiceApi
{
    Task<ServiceCallResult<LoginResult>> Login(LoginRequest request);
    Task<ServiceCallResult> Logout(LogoutRequest request);
    Task<ServiceCallResult<IsUserLoggedInResult>> IsUserLoggedIn(IsUserLoggedInRequest request);
}
```

Clearly, this service will have a method for logging a user in, for logging him out, and for checking his login status. 

Here are the request and result entities that we'll need. 

```
public class LoginRequest {
    public string Username { get; set; }
    public string Password { get; set; }
}
```
```
public class LoginResult {
    public bool UserLoggedIn { get; set; }
}
```
```
public class LogoutRequest {
    public string Username { get; set; }
}
```
```
public class IsUserLoggedInRequest {
    public string Username { get; set; }
}
```
```
public class IsUserLoggedInResult {
    public bool UserLoggedIn { get; set; }
}
```

### Tutorial.Session.Client

The Client project should be pretty simple.  Follow what you did for the User service to create Tutorial.Session.Client.

### Tutorial.Session

The Tutorial.Session project is where the service code will live.  This will be a bit different this time.  The User service only had an ApiOperation.cs which contained the Operation logic.  But now, we'll need to do some magic using a Service class as well.  

#### The Service interface

First of all, let's define an interface for our Service class.  This interface will expose some functionality that the Operation will need.  

Create a file call ISessionService.cs:

```
using System;
using System.Collections.Concurrent;
using XKit.Lib.Common.Host;

namespace Tutorial.Session;

public interface ISessionService : IManagedService
{
    ConcurrentDictionary<string, DateTime> GetSessions();
}
```

As you can see, there is just one method that returns a ConcurrentDictionary.  The dictionary will be keyed by a string (which will be the username) and have values of DateTime when they last logged in. 

#### The Service class

Now let's create the Service class.  Create a file called SessionService.cs and put the following in it. 

```
using System;
using System.Collections.Concurrent;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;
using XKit.Lib.Host.DefaultBaseClasses;

namespace Tutorial.Session;

public class SessionService : ManagedService<ApiOperation>, ISessionService
{
    private ConcurrentDictionary<string, DateTime> sessions = new();
    
    public SessionService(IXKitHostEnvironment hostEnv) 
        : base(hostEnv)
    {
    }

    protected override IReadOnlyDescriptor Descriptor => Constants.ServiceDescriptor;

    public ConcurrentDictionary<string, DateTime> GetSessions() {
        return sessions;
    }
}
```

What do we notice here?  The Service class derives from `ManagedService` and takes our Operation type as a type parameter.  It also implements `ISessionService`.  So far so good.  This is the first requirement for a Service class. 

It also overrides `IReadOnlyDescriptor Descriptor` to return it's service descriptor which describes which service it is.  This is the second requirement for our Service class. 

So to create a Service class is pretty straightforward.  

Finally, it simply returns the class level `session` as the implementation of `GetSessions()`.  As we'll see, our Operation will need that. 

#### The Service Operation

Let's create our Operation.  Create a file called ApiOperation.cs.  On your own, create this class according to what we did before with User.  You'll need to provide an implementation for the three service methods in our interface.  

Once you are done with the basic Operation class, we'll add in the logic for `DoLogin`, `DoLogout`, and `DoIsUserLoggedIn`

Our Session service will have User as a _dependency_.  So Session will _call_ the User service.  In order to that, we'll need to add a reference to the Tutorial.User.Client project and the Tutorial.User.Common.  This is because Session will be using the User Client.

The first thing we'll need to do is add the reference to our Tutorial.Session.csproj file.  Add these lines:

```
<ProjectReference Include="../Tutorial.User.Client/Tutorial.User.Client.csproj"/>
<ProjectReference Include="../Tutorial.User.Common/Tutorial.User.Common.csproj"/>
```

Now let's add the implementation for `DoLogin`

```
private async Task<LoginResult> DoLogin(LoginRequest request) 
{
    var userClient = new Tutorial.User.Client(Connector);
    var getUserResult = await userClient.GetUser(new() { Username = request.Username });
    if (getUserResult.HasError) { 
        Log.Error("Can't find that user!");
        return null;
    }

    // check for bad password
    if (getUserResult.ResponseBody.Password != request.Password) {
        Log.Audit("Invalid user credentials!");
        return new() { UserLoggedIn = false };
    }

    // if we got this far, we are good!

    var sessions = Service.GetSessions();
    sessions[request.Username] = DateTime.Now;
    return new() { UserLoggedIn = true };
}
```

_What just happened?_  The first thing our method does is to create a Client object to be able to call User.  Then it uses this to call `GetUser` and takes this information to check the password.  If the user exists and the password matches, then we use our `GetSession` from the Service class to get our ConcurrentDictionary and add the user to it.  Finally, we return our response. 

One detail to notice above:  If `GetUser` returns an error, we log an error and return a null result.  Doing this will cause the current service call to also fail with an error.  Generally you would not want to do this when logging a user in (a non-existent user should return the same response as bad credentials) but in this case it illustrates how to error out the current service call. 

Let's do the next one.  This is the implementation for `DoLogout`
```
private Task DoLogout(LogoutRequest request) 
{  
    var sessions = Service.GetSessions();
    sessions.TryRemove(request.Username, out var _);
    return Task.CompletedTask;
}
```

It should be pretty easy to see what is happening there. We simply get our ConcurrentDictionary and remove the user if he has an entry. 

Finally `DoIsUserLoggedIn`
```
private Task<IsUserLoggedInResult> DoIsUserLoggedIn(IsUserLoggedInRequest request) 
{   
    var sessions = Service.GetSessions();
    var isLoggedIn = sessions.ContainsKey(request.Username);
    return Task.FromResult(new IsUserLoggedInResult { UserLoggedIn = isLoggedIn });
}
```

That should be pretty clear what is going on. 

Do you see why we needed to define a method on the Service level?  The `sessions` information needs to last longer than a single operation lifetime.  It needs lives for the duration of the _service_ lifetime.  When designing services, it is important to understand clearly what resources are very short lived (and therefore stay within the Operation) and what are longer lived and therefore managed by the main Service code.  

One more thing we need to do:  The Session Service needs to declare it's dependencies.  This is as simple as adding an override to the Service class.  Go back to your `SessionService.cs` file and add this to the `SessionService` class:

```
protected override IEnumerable<IReadOnlyDescriptor> Dependencies => new[] {
    User.Constants.ServiceDescriptor
};
```

By overriding `Dependencies` and returning an array with the Descriptor for User in it, we are declaring that User is a dependency of Session. 

### Tutorial.Session.Test

You should be able to write most of the tests for yourself.  The one method that will not be possible to write tests for with your current level of knowledge is `Login`.  Why?  Because `Login` calls the User service.  If you try to test it without taking this into account, your test will inevitably fail.  This is because in the test environment, there is no User service.  

There are two ways to handle this, as is indicated [in the docs about testing](../../docs/GettingStarted-TestingServiceDependencies.md).  We could simply include the User service in our Test project and let it actually function in our test environment.  This would be an "Integration" test, or a test that actually integrates dependencies.  In a small example such as this, an integration test would be easy to do.  However with larger and larger systems, integration tests become more complex (although they definitely still have a place and are part of a good testing strategy).  

In this case, we want to do a Unit test, which isolates our "unit" (our service method) and tests it with mocked dependencies.  So we have to provide a "mock" or fake `GetUsers` call in order to return pretend data.  

Let's get started with our test for `Login`. 

#### Set up the test

Let's say that we want to test `Login` for two cases:  1) When a user provides correct login credentials, and 2) When a user provides an incorrect password.  

First, create the Tutorial.Session.Test project according to the pattern you've already seen when doing previous tests.  

Use this for the Tutorial.Session.Test.csproj file: 

```
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>

    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.2.0" />
    <PackageReference Include="MSTest.TestAdapter" Version="2.2.10" />
    <PackageReference Include="MSTest.TestFramework" Version="2.2.10" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="XKit.Lib.Testing" Version="*" />
    <ProjectReference Include="../Tutorial.Session.Common/Tutorial.Session.Common.csproj" />
    <ProjectReference Include="../Tutorial.Session.Client/Tutorial.Session.Client.csproj" />
    <ProjectReference Include="../Tutorial.Session/Tutorial.Session.csproj" />
  </ItemGroup>

</Project>
```

Let's create a file called `LoginTests.cs` as a place to put our tests for `Login`. 

```
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using XKit.Lib.Testing;
using Tutorial.User;
using XKit.Lib.Common.Fabric;

namespace Tutorial.Session.Test;

[TestClass]
public class LoginTests
{
    private static readonly TestHostHelper testHelper = new();

    [ClassInitialize]
    public static void ClassInit(TestContext context) {
        testHelper.InitializeLocalTestHost();
        testHelper.AddService(new SessionService(testHelper.HostEnvironment));
        testHelper.StartHost();
    }

    [ClassCleanup]
    public static void ClassTeardown() {
        testHelper.DestroyHost();
    }
```

This should mostly be familiar to you.  The one thing that is a bit different here is that in the User service tests, we added the service with a call to `testHelper.AddCreateService()`.  Here we are calling a slightly different method `testHelper.AddService()`.  Why?  The User service only has an Operation.  It does not have a Service class.  That makes it a "generic" service.  But in the case of Session, we need to instantiate an instance of the Session Service class since it has one. 

Now let's add a few tests.  Let's start with a test to prove that logging in with the correct password works as expected.  Add this method:

```
[TestMethod]
public async Task LoginSucceedsWithCorrectPassword()
{
    var client = new Client(testHelper.HostEnvironment.Connector);

    var result = await client.Login(new() { 
        Username = "alice",
        Password = "pwd"
    });

    Assert.IsFalse(result.HasError);
    Assert.IsTrue(result.ResponseBody.UserLoggedIn);
}
```

That seems pretty straightforward.  We create a Client to talk to our Session service, then we call `Login` and check the results.  Let's do something similar for the case of a bad password: 

```
[TestMethod]
public async Task LoginSucceedsWithIncorrectPassword()
{
    var client = new Client(testHelper.HostEnvironment.Connector);

    var result = await client.Login(new() { 
        Username = "kermit",
        Password = "a bad password"
    });

    Assert.IsFalse(result.HasError);
    Assert.IsFalse(result.ResponseBody.UserLoggedIn);
}
```

Again, that should be pretty clear.  

There is only one problem.  At this point, if we were to run the tests they would fail because the our call to `Login` in the Session operation tries to call `GetUsers`, and up until now the User service does not exist in our testing environment.  Let's fix that now.  

#### Add a reference to the User service

In the Tutorial.Session.Test.csproj file, add the following reference: 

```
<ProjectReference Include="../Tutorial.User.Common/Tutorial.User.Common.csproj"/>
```

Why do we need this?  Because Session depends on User, but this is a _unit_ test.  The Session service needs to be tested _apart from_ the User service that it depends on.  In order to test Session, we will have to mock the `IUserApi` interface.  

There are two approaches to doing this:  1) Mocking with a fake User service or 2) mocking with the Moq library.

#### Mocking with a fake User service

One approach for doing this is to create a "fake" User service only for the purpose of testing Session.  Then we include this "fake" User service in the testing environment. 

In the Tutorial.Session.Test project, create a new file called `MockUser.cs`.  We are going to create an Operation here that pretends to be the Operation for the User service.  

Add this content to `MockUser.cs`

```
using System;
using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Log;
using XKit.Lib.Host.DefaultBaseClasses;

namespace Tutorial.Session.Test;

public class MockUserOperation : ServiceOperation, User.IUserApi
{
    public MockUserOperation(ServiceOperationContext context) 
        : base(context)
    {
    }

    public Task<ServiceCallResult<Tutorial.User.User>> GetUser(Tutorial.User.User request) 
    {
        return RunServiceCall(
            request,
            operationAction: (req) => {
                // success case
                if (request.Username == "alice") {
                    return Task.FromResult(new OperationResult<Tutorial.User.User> {
                        OperationStatus = LogResultStatusEnum.Success,
                        ResultData = new Tutorial.User.User {
                            Email = "alice@tutorial.com",
                            Username = "alice",
                            FullName = "Alice TheUser",
                            Password = "pwd"
                        }
                    });
                }
                // failure case
                return Task.FromResult(new OperationResult<Tutorial.User.User> {
                    OperationStatus = LogResultStatusEnum.NonRetriableError,
                    Message = "User not found"
                });
            }
        );
    }

    public Task<ServiceCallResult> UpsertUser(User.User request) {
        throw new NotImplementedException();
    }

    public Task<ServiceCallResult> DeleteUser(Tutorial.User.User request) 
    {
        throw new NotImplementedException();
    }
}
```

What's going on there?  We just created an operation that implements `IUserApi` so it can stand in for the User operation.  Session doesn't try to call `UpsertUser` or `DeleteUser`, so there is no need to provide any implementation for those methods.  However our `Login` method _will_ call `GetUser` so we provide an implementation that returns some mocked data.  If the user being requested is "alice" then it returns mocked data for Alice.  Otherwise, it indicates the user was not found.  

Now let's tell our test environment to use that mocked operation.  In our `LoginTests.cs` file, add this line to the `ClassInit()`

```
[ClassInitialize]
public static void ClassInit(TestContext context) {
    testHelper.InitializeLocalTestHost();
    testHelper.AddService(new SessionService(testHelper.HostEnvironment));
    testHelper.AddCreateService(   // <--- add this 
        User.Constants.ServiceDescriptor,
        typeof(MockUserOperation)
    );
    testHelper.StartHost();
}
```

We are adding it in our test environment exactly the same way we would add it to a Host.  In fact, it is being added to the test Host.  Because are are telling the test Host that this is the User service (because we are passing the Descriptor of the User service), it will call the methods on `MockUserOperation`.
 
If you now run the tests for `Login`, they should pass!  For even more fun, put a breakpoint in our mock User operation and debug the tests, and you will execution pause there exactly as you would expect. 

#### Mocking with a fake User service

Using a fake service is a perfectly acceptable way to mock dependencies.  An alternative is to use the library Moq, which lets us set up mocks without the need to create a separate class. 

To start with, comment out the line we added to `ClassInit` so that we can try the other way of adding mocks.  

```
[ClassInitialize]
public static void ClassInit(TestContext context) {
    testHelper.InitializeLocalTestHost();
    testHelper.AddService(new SessionService(testHelper.HostEnvironment));
    //testHelper.AddCreateService(   // <--- comment this out
    //    User.Constants.ServiceDescriptor,
    //    typeof(MockUserOperation)
    //);
    testHelper.StartHost();
}
```

Then in the same method, add the following:

```
var userMock = testHelper.AddMockService<IUserApi>(User.Constants.ServiceDescriptor);
// success case
userMock.ApiMock
    .Setup(x => x.GetUser(It.Is<Tutorial.User.User>(x => x.Username == "alice")))
    .ReturnsAsync(new ServiceCallResult<Tutorial.User.User> { 
        OperationStatus = XKit.Lib.Common.Log.LogResultStatusEnum.Success,
        ServiceCallStatus = ServiceCallStatusEnum.Completed,
        ResponseBody = new Tutorial.User.User {
            Email = "alice@tutorial.com",
            Username = "alice",
            FullName = "Alice TheUser",
            Password = "pwd"
        }
    });
// failure case
userMock.ApiMock
    .Setup(x => x.GetUser(It.Is<Tutorial.User.User>(x => x.Username != "alice")))
    .ReturnsAsync(new ServiceCallResult<Tutorial.User.User> { 
        OperationStatus = XKit.Lib.Common.Log.LogResultStatusEnum.NonRetriableError,
        Message = "User not found",
        ServiceCallStatus = ServiceCallStatusEnum.Completed,
    });
```

If you have not used Moq before, that can look quite confusing.  It is simply saying that in the case of the user name being "alice", return one result and if not, return the other.  

One thing to note:  When using Moq, the response you are returning is slightly different.  In this case it is a `ServiceCallResult` whereas before in the fake service it was an `OperationResult`.  Why the difference?  Simply put it has to do with where we are doing the work of mocking.  In the fake service, you are actually creating a service to use.  `RunServiceCall` translates the more general `OperationResult` into a `ServiceCallResult` just like it would for a "real" service call. 

## Step 2:  Add the Session service to the Host

Now that we have our tests out of the way (at least as far as `Login` is concerned), the last step is to add the service to the Host.  If you go back to the Tutorial.Host1 project, let's do that.  

First, add the project references to the csproj file: 

```
<ProjectReference Include="../Tutorial.Session.Common/Tutorial.Session.Common.csproj" />
<ProjectReference Include="../Tutorial.Session/Tutorial.Session.csproj" />
```

Then open `Program.cs` and add this line below where we are adding the User service.

```
host.AddManagedService(new Tutorial.Session.SessionService(host));
```

## Step 3:  Call the Session service

If everything has happened as expected so far, the Host should be ready to run both services.  When we `Login`, it will automatically find the User service.  Let's try it.  

Run the host using perhaps `dotnet run` in the Tutorial.Host1 folder and then from the bash prompt let's run a `curl` command to log a user in.  If we are using the same data as before, then the user "kermit" should have a password "ribbot!"

```
$ curl -d '{"operationName":"Login","payloadObj":{"username":"kermit","password":"ribbot!"}}' -H "Content-Type: application/json" -X POST http://localhost:5000/managed/Tutorial/Session/1
```

Assuming that you previously created the user, the result should be something like this:
```
{
  "serviceCallStatus": "Completed",
  "operationStatus": "Success",
  "serviceStatus": {
    "instanceId": "416621270b8f4bd0ad514010dcd3230b",
    "availability": "Serving5",
    "health": "Unknown",
    "runState": "Active"
  },
  "message": null,
  "responderInstanceId": "416621270b8f4bd0ad514010dcd3230b",
  "responderFabricId": "a9d6bfb1d5de439fbabe92c3f4e35f89",
  "operationId": "8d0d5476-4385-49bb-9443-34c9289f9cd9",
  "operationName": "Login",
  "service": {
    "isMetaService": false,
    "collection": "Tutorial",
    "name": "Session",
    "version": 1,
    "updateLevel": 0,
    "patchLevel": 0
  },
  "correlationId": "3d1adabfe4284715b90e5d3c84f2b86f",
  "requestorInstanceId": null,
  "requestorFabricId": null,
  "code": null,
  "timestamp": "2022-08-03T19:34:46.4079696Z",
  "payloadObj": {
    "userLoggedIn": true
  }
}
```

IF you were to place a breakpoint in `GetUser` and run the above, you would see that the debugger stops there during the call, which is coming from `Login`.

## Your mission, should you choose to accept it...

* There is one more test case that we really should test for in `LoginTests.cs`.  Can you see what it is?  Add the additional test.
* Try to write tests for `Logout` and `IsUserLoggedIn`.  Use what we did for the User service as a guide.  

The project in this folder contains the final version.

## Final thoughts

At this point, you should have a basic grasp of the process of creating a service with an API, how hosts work, and how a service can depend on another.    


[Go to the next tutorial:  TBD](../Tutorial003_Calling_One_Service_From_Another/)



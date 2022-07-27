# Tutorial 2:  Organize the Project and Add the Client and Tests

[TOC]

## Description and objectives

This tutorial will:

* Walk you through a recommended way of organizing a service project.  
* Walk you through a creating a client to access the service API methods from a consumer. 
* Walk you through adding tests
* Set you loose to finish the client and tests. 

Continuing with our service for managing users, we will make the project more useful and apply some good practices that were glossed over in Tutorial 1 (like tests!).  

Please note:  This tutorial will ask you to do a number of things "on your own" to allow you to learn.  If you get stuck, you have the "finished" code as a reference. 

## Folder structure

Eventually we should have a folder structure that looks like this:
```
(root/home)
|
├───Tutorial.Host1
|       Program.cs
|       Tutorial.Host1.csproj
├───Tutorial.User
|       ApiOperation.cs
|       Tutorial.User.csproj
├───Tutorial.User.Client
|       Client.cs
|       Tutorial.User.Client.csproj
├───Tutorial.User.Common
|       Constants.cs
|       IUserApi.cs
|       Tutorial.User.Common.csproj
|       User.cs
└───Tutorial.User.Test
        Tutorial.User.Test.csproj
        UpsertUserTest.cs
```

## Prerequisites

* This tutorial will assume that you are using the Linux command line (also available in Windows!)
* You either previously went through Tutorial 1 or else copied the code from the repository.

## Concepts

These are the concepts that you should understand as you begin this tutorial:

* A _Consumer_ is some actor (maybe another service or some other code) that accesses the functionality provided by a service through its api.  
* A _Client_ is an object which allows a consumer to easily call a service by taking care of all the details of communication. 
* A _Test_ is just what the name implies.  It’s code that verifies that a class or method works as it should.

## Step 1:  Create the Common, Client and Test projects

We already have our Tutorial.User project.  Now we need to create a few more.  

### Create the Common project and move stuff into it.

In Tutorial 1, we put everything together in one project.  But really, it's better to separate out the interfaces, entities, constants, etc.  Let's do that now, putting that stuff into a Common library.

We'll use a library project, because this will just be an assembly that other code uses.  Then we'll move our api interface and User entity into it.

From the parent folder that contains `Tutorial`...

```
:~/$ mkdir Tutorial.User.Common
:~/$ cd Tutorial.User.Common
:~/Tutorial.User.Common$ dotnet new classlib
:~/Tutorial.User.Common$ mv ../Tutorial.User/IUserApi.cs . 
:~/Tutorial.User.Common$ mv ../Tutorial.User/User.cs . 
```

Edit the `Tutorial.User.Common.csproj` file and add a reference to `XKit.Lib.Common`:

```
  <ItemGroup>
    <PackageReference Include="XKit.Lib.Common" Version="*"/>
  </ItemGroup>
```

At this point the project should build.

```
:~/Tutorial.User.Common$ dotnet build 
```

Let's do one more thing.  We also should move into this project the "Service Descriptor" that describes the name of the service and it's version.   

In the same folder, create a file called "Constants.cs".  (You can delete any .cs files that were put there by `dotnet new`)

Then edit the Constants.cs file to look like this:

```
using XKit.Lib.Common.Registration;

namespace Tutorial.User;
public static class Constants
{
    public static readonly Descriptor ServiceDescriptor = new Descriptor {
        Collection = "Tutorial",
        Name = "User",
        Version = 1
    };
}
```

Finally, build Tutorial.Common again to make sure it builds.

```
:~/Tutorial.User.Common$ dotnet build 
```

NOTE:  If you are using VS Code as your editor, sometimes it can get confused with a bunch of changes.  If you see errors in VS Code, but the build is fine from the command line, then restart Omnisharp.  In VS Code, hit Ctrl-Shift-P and type "Restart Omnisharp" and click it. 

### Point the Tutorial.User project at the new Tutorial.User.Common

Now that we did all that, our original Tutorial.User is broken, because it doesn't know where to find `IUserApi.cs` or `User.cs`.  Let's fix that. 

Edit the `Tutorial.User.csproj` file and add the following to one of the `<ItemGroup>` sections:

```
    <ProjectReference Include="../Tutorial.User.Common/Tutorial.User.Common.csproj"/>
```

And build it.  From the `Tutorial.User` folder:  
```
:~/Tutorial.User.Common$ dotnet build 
```

And if everything happened as it should, it should build correctly.  

There's one more tiny detail.  Remember how we had to pass the "service descriptor" when we created the `User` service?  In `Program.cs`:

```
host.AddCreateManagedService(
    serviceDescriptor: new Descriptor {
        Collection = "Tutorial",
        Name = "User",
        Version = 1
    },
    typeof(IUserApi)
);
```

Well, we have that information in the `Tutorial.User.Common` project now.  We should just reference that.  
```
host.AddCreateManagedService(
    serviceDescriptor: Constants.ServiceDescriptor,
    typeof(IUserApi)
);
```

We should be good to go with no errors.  If you see a bunch of weird errors, try restarting Omnisharp (see above) before panicking.

## Step 2:  Create the Client

### Create the project 

Now that we have that bit of housekeeping out of the way, let's create a _Client_ for our service.  The client will allow other C# code (other services or just any code) to access our `User` service.  

We'll use a library project again.  From the parent folder that contains `Tutorial.User` and `Tutorial.User.Common`...

```
:~/$ mkdir Tutorial.User.Client
:~/$ cd Tutorial.User.Client
:~/Tutorial.User.Client$ dotnet new classlib
```

Edit the `Tutorial.User.Client.csproj` file and add XerviceKit references, as well as a reference to our Tutorial.User.Common project:

```
  <ItemGroup>
    <PackageReference Include="XKit.Lib.Connector" Version="*"/>
    <PackageReference Include="XKit.Lib.Connector.Protocols.Direct" Version="*"/>
    <PackageReference Include="XKit.Lib.Connector.Protocols.Http" Version="*"/>
    <ProjectReference Include="../Tutorial.User.Common/Tutorial.User.Common.csproj"/>
  </ItemGroup>
```

### Create the Client class

In the same folder, create a file called "Client.cs" (you can get rid of any files .cs that `dotnet new` places there by default).  The Client class will just be a thin wrapper that makes service calls for us.  It will implement our IUserApi interface as well. 

Make it look like this:

```
using System;
using System.Threading.Tasks;
using XKit.Lib.Common.Client;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Registration;
using XKit.Lib.Connector.Service;

namespace Tutorial.User;
public class Client : ServiceClientBase<IUserApi>, IUserApi
{
    public Client()
        : base(Constants.ServiceDescriptor)
    {
    }

    public Task<ServiceApiResult> UpsertUser(User request)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceApiResult> DeleteUser(User request)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceApiResult<User>> GetUser(User request)
    {
        throw new NotImplementedException();
    }
}
```

We'll fill in some code in a minute.  For now, make sure the project builds (use dotnet build).

### Add code to the Client class

Now let's add in a tiny bit of wiring to let this class talk to the service.  Modifying the `UpsertUser` and `GetUser` methods in the `Client` class...

```
public Task<ServiceApiResult> UpsertUser(User request)
{
    // ----->
    return this.ExecuteCall(request);
    // <-----
}
...
public Task<ServiceApiResult<User>> GetUser(User request)
{
    // ----->
    return this.ExecuteCall<User, User>(request);
    // <-----
}
```
_What just happened?_  

In both cases, we call the `ExecuteCall` method from the base class, and it does all the work of finding the service and calling it's api.  How does it know what method to call?  It looks at it's own containing method (in the case of upserting the user the method is called obviously `UpsertUser` and from that it knows the name of the service api method to call.   

(Side note:  In the case of `UpsertUsers`, the compiler could figure out what types are needed for the generic `ExecuteCall`.  But in the case of `GetUsers`, the compiler can't figure it all out on it's own, so we specify `<User, User>`.  That's just saying that we expect to use a request object of `User` and we expect a response also of type `User`.)

Build the project one more time, and that's it.  We will use this project a bit later when we do the test. 

## Step 3:  Create the Test

### Create the project 

Good automated tests are a foundation cornerstone of maintainable software.  Thankfully, they are easy to write.  Let's create a test project for our service and add some tests to it.  The test will use the _client_ that we just created.

A quick note about testing:  There are different philosophies about the user of _Unit_ tests versus _Integration_ or _System_ tests versus _Functional_ tests.  Any are possible using XerviceKit.  In this case, we'll use a kind of _Unit_ test approach where the "units" in question are the service operation methods.  Really though, as you will see, it is possible to do a kind of hybrid test that allows for less complicated tests at the same time as combining some strengths of _Unit_ and _Integration_ tests.  This will be discussed in a later tutorial.

Lets create a test project.  From the parent folder that contains `Tutorial.User`...

```
:~/$ mkdir Tutorial.User.Test
:~/$ cd Tutorial.User.Test
:~/Tutorial.User.Test$ dotnet new mstest
```

Edit the `Tutorial.User.Test.csproj` file and add XerviceKit references, as well as a references to our other projects:

```
  <ItemGroup>
    <PackageReference Include="XKit.Lib.Testing" Version="*"/>
    <ProjectReference Include="../Tutorial.User.Common/Tutorial.User.Common.csproj"/>
    <ProjectReference Include="../Tutorial.User.Client/Tutorial.User.Client.csproj"/>
    <ProjectReference Include="../Tutorial.User/Tutorial.User.csproj"/>
  </ItemGroup>
```

### Create the Test class

In the same folder, rename the default file that dotnet placed there for us when we created the project.  Let's rename it to "UpsertUserTests.cs".  This class will contain all the tests for `UpsertUser`.

```
:~/Tutorial.User.Test$ mv UnitTest1.cs UpsertUserTests.cs 
```

Let's start with the basic layout of the class. 

```
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XKit.Lib.Testing.Integration;

namespace Tutorial.User.Test;

[TestClass]
public class UpsertUserTests
{
}
```

Now let's add some code to so that this test class can test our service api method.

### Add class init and cleanup 

Add the following two methods to the test class:
```
private static readonly TestHostHelper testHelper = new();

[ClassInitialize]
public static void ClassInit(TestContext context) {
    testHelper.InitializeLocalTestHost();
    testHelper.AddCreateService(
        Constants.ServiceDescriptor,
        typeof(ApiOperation)
    );
    testHelper.StartHost();
}

[ClassCleanup]
public static void ClassTeardown() {
    testHelper.DestroyHost();
}
```

_What just happened?_ 

The tests will occur in a "real" service environment.  So the test helper is doing some behind the scenes magic to initialize our host, add our service, and start it.  It is also destroying it appropriately at the end of all the tests.   

Now we are ready to add the a test.  Let's start with one that checks to see if `UpsertUser` correctly creates a new user.  

```
[TestMethod]
public async Task UpsertWithNewUserSucceeds()
{
    var client = new Client();

    var upsertResult = await client.UpsertUser(new User { 
        Username = "kermit",
        Email = "kermit@thefrog.com",
        FullName = "Kermit TheFrog"
    });

    Assert.IsFalse(upsertResult.HasError);

    var getResult = await client.GetUser(new User {
        Username = "kermit"
    });

    Assert.AreEqual(getResult.ResponseBody.Username, "kermit");
    Assert.AreEqual(getResult.ResponseBody.Email, "kermit@thefrog.com");
    Assert.AreEqual(getResult.ResponseBody.FullName, "Kermit TheFrog");
}
```

_What just happened?_ 

1) We create a _client_ that lets us talk to the service.  The service is already up and running because of the `ClassInit` code. 

2) We call the service api method `UpsertUser` using the `client`.  Magic happens.

3) We check the result to see if there was an error.  So far so good!

4) We use the `client` again to call `GetUsers` to get the use we just created. 

5) We verify that the returned user has the data we expect. 

Build the project and run the test.  It should pass!

Let's add one more before we finish up. 

```
[TestMethod]
public async Task UpsertWithExistingUserSucceeds()
{
    var client = new Client();

    var upsertResult1 = await client.UpsertUser(new User { 
        Username = "kermit",
        Email = "kermit@thefrog.com",
        FullName = "Kermit TheFrog"
    });

    var upsertResult2 = await client.UpsertUser(new User { 
        Username = "kermit",
        Email = "kermie@frogger.com",
        FullName = "Kermit D Frog"
    });
    Assert.IsFalse(upsertResult2.HasError);

    var getResult = await client.GetUser(new User {
        Username = "kermit"
    });

    Assert.AreEqual(getResult.ResponseBody.Username, "kermit");
    Assert.AreEqual(getResult.ResponseBody.Email, "kermie@frogger.com");
    Assert.AreEqual(getResult.ResponseBody.FullName, "Kermit D Frog");
}
```

_What just happened?_  

1) We create a _client_ again that lets us talk to the service.  

2) We call the service api method `UpsertUser` using the `client` to insert the user.

3) We check the result to see if there was an error.  

4) We call the service api method `UpsertUser` again to with different data to update the user.

5) We use the `client` again to call `GetUsers` to get the use we just updated. 

6) We verify that the returned user has updated the data. 

There is a small problem with both of these tests.  Maybe you noticed it?  It is possible if tests are run more than once, or if one is run after the other, that existing data might be present from a previous test run.  To fix this, we would want to clear any existing data at the start of each test.  For now though, we will leave it. 

## Step 4:  Separate out the host from the service

When we did Tutorial 1, for convenience we put the host code in the same project as the service.  But really, a host and a service don't belong together.  Why can we say that?  The two have very different purposes.  The host provides an environment for a service to run in, but really any service or more than one service might run in a host.  The service on the other hand is does very specific work and provides specific functionality. 

Let's separate them out into two different projects.  Start by creating a project for the host.  From the parent folder that contains the tutorial...

```
:~/$ mkdir Tutorial.Host1
:~/$ cd Tutorial.Host1
:~/Tutorial.Host1$ dotnet new web
```

Edit the `Tutorial.Host1.csproj` file and add XerviceKit references, as well as a references to our other projects.  Notice that we include a reference to our service project and common project:

```
  <ItemGroup>
    <PackageReference Include="XKit.Lib.Host" Version="*"/>
    <PackageReference Include="XKit.Lib.Host.Helpers" Version="*"/>
    <PackageReference Include="XKit.Lib.Host.Protocols.Http" Version="*"/>
    <ProjectReference Include="../Tutorial.User.Common/Tutorial.User.Common.csproj" />
    <ProjectReference Include="../Tutorial.User/Tutorial.User.csproj" />
  </ItemGroup>
```

Now move over the following `Program.cs` from Tutorial.User to Tutorial.Host1

```
:~/Tutorial.User.Test$ mv ./Tutorial.User/Program.cs ./Tutorial.Host1/ 
```

Also, edit `Program.cs` and change the namespace to `Tutorial.Host1` and add a using `using Tutorial.User`.  

It should look like this at the end:
```
using Tutorial.User;
using XKit.Lib.Common.Host;
using XKit.Lib.Host;
using XKit.Lib.Host.Protocols.Http;

var builder = WebApplication.CreateBuilder(args);

HostEnvironmentHelper hostHelper = new();
var host = hostHelper.CreateInitHost(hostAddress: "localhost");

host.AddCreateManagedService(
    Constants.ServiceDescriptor,
    typeof(ApiOperation)
);

builder.Services.AddScoped<IXKitHostEnvironment>(_ => hostHelper.Host);
builder.Services.AddMvcCore().AddXKitHostMvc();
builder.Services.AddControllers();

var app = builder.Build();
app.UseRouting();
app.UseEndpoints(endpoints => endpoints.MapControllers());

hostHelper.StartHost();
app.Run();
hostHelper.StopAndDestroyHost();
```

When you are done, the only code file left in Tutorial.User should be `ApiOperation.cs`. 

One more thing:  Our old `Tutorial.User.csproj` file is no longer a web application, since all that is now in `Tutorial.User.Host1`.  We have to make a small change to it in order for it to build.  Open `Tutorial.User.csproj` and change the SDK target from `Microsoft.NET.Sdk.Web` to `Microsoft.NET.Sdk`.

When you are done, `Tutorial.User.csproj` should look like this:
```
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="XKit.Lib.Host" Version="*"/>
    <PackageReference Include="XKit.Lib.Host.Helpers" Version="*"/>
    <PackageReference Include="XKit.Lib.Host.Protocols.Http" Version="*"/>
    <ProjectReference Include="../Tutorial.User.Common/Tutorial.User.Common.csproj"/>
  </ItemGroup>

</Project>
```

One last thing needs to be done.  Since Tutorial.User is not longer a web project (because we moved all that stuff to Tutorial.Host1), it won't build unless we tweak the csproj slightly.  

```
<Project Sdk="Microsoft.NET.Sdk">
```

Now try to build `Tutorial.Host1` and it should build. 

## Step 5 (optional):  Fix `launch.json` and `tasks.json`

If you have a `.vscode` folder (created optionally by VsCode), and in it you have a `tasks.json` and a `launch.json`, then edit them to point to the new Tutorial.Host1 project instead of Tutorial.User

In `tasks.json`, replace `Tutorial.User` with `Tutorial.Host1`.
Do the same thing for `launch.json`.

## Step 6 (optional): Sanity check

If you want to do a quick sanity check, you should be able to run Tutorial.Host1 and the curl command from before should work once again:

```
curl -d '{"operationName":"UpsertUser","payloadObj":{"username":"kermit","email":"kermit@frogville.com","fullName":"Kermit TheFrog","password":"ribbot!"}}' -H "Content-Type: application/json" -X POST http://localhost:5000/managed/Tutorial/User/1
```

## Your mission, should you choose to accept it...

* Using the same pattern as we started above, implement the method `DeleteUsers` in `Client` class.
* Implement tests for `GetUsers` and `DeleteUsers`.  Of course, you will have to think of test cases for each of them.
* Fix the problem mentioned in the tests where data can be left over.  It's actually quite easy.  See if you can figure out how to do it.

The project in this folder contains the final version.

## Final thoughts

There was a lot of stuff going on in this tutorial.  You may have gotten lost along the way.  If you did, no worries.  You can use finished code in this folder as a guide.  

At this point, the service we have written is now usable.  It runs, and code can talk to it.  It has been proved to work via tests.  

This tutorial had a lot of boring boilerplate stuff again, like creating projects, etc.    The main points to take away are in the guts of Step 2 where we create the Client and in Step 3 where we create tests.  

[Go to the next tutorial:  Calling one service from another (TBD)]()



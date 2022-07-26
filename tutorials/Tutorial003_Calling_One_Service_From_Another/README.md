# Tutorial 2:  Organize the Project and Add the Client and Tests

[TOC]

## Description and objectives

This tutorial will:

* Walk you through a recommended way of organizing a service project.  
* Walk you through a client to access the service API methods from a consumer. 
* Walk you through adding tests
* Set you loose to finish the client and tests. 

Continuing with our service for managing users, we will make the project more useful and apply some good practices that were glossed over in Tutorial 1 (like tests!).  

## Folder structure

Eventually we should have a folder structure that looks like this:
```
(root/home)
|
├───Tutorial.Session
|   └───Properties
|           launchSettings.json
|       ApiOperation.cs
|       appsettings.Development.json
|       appsettings.json    
|       Program.cs
|       Startup.cs
|       Tutorial.Session.csproj
├───Tutorial.Session.Client
|       Client.cs
|       Tutorial.Session.Client.csproj
├───Tutorial.Session.Common
|       Constants.cs
|       ISessionApi.cs
|       Tutorial.Session.Common.csproj
|       Session.cs
└───Tutorial.Session.Test
        Tutorial.Session.Test.csproj
        UpsertSessionTest.cs
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

We already have our Tutorial.Session project.  Now we need to create a few more.  

### Create the Common project and move stuff into it.

In Tutorial 1, we put everything together in one project.  But really, it's better to separate out the interfaces, entities, constants, etc.  Let's do that now, putting that stuff into a Common library.

We'll use a library project, because this will just be an assembly that other code uses.  Then we'll move our api interface and Session entity into it.

From the parent folder that contains `Tutorial`...

```
#!bash
:~/$ mkdir Tutorial.Session.Common
:~/$ cd Tutorial.Session.Common
:~/Tutorial.Session.Common$ dotnet new classlib
:~/Tutorial.Session.Common$ mv ../Tutorial.Session/ISessionApi.cs . 
:~/Tutorial.Session.Common$ mv ../Tutorial.Session/Session.cs . 
```

Edit the `Tutorial.Session.Common.csproj` file and add a reference to `XKit.Lib.Common`:

```
#!XML
  <ItemGroup>
    <PackageReference Include="XKit.Lib.Common" Version="*"/>
  </ItemGroup>
```

At this point the project should build.

```
#!bash
:~/Tutorial.Session.Common$ dotnet build 
```

Let's do one more thing.  We also should move into this project the "Service Descriptor" that describes the name of the service and it's version.   

In the same folder, rename the default file that dotnet placed there for us when we created the project.  Let's rename it to "Constants.cs"

```
#!bash
:~/Tutorial.Session.Common$ mv Class1.cs Constants.cs 
```

Then edit the Constants.cs file to look like this:

```
#!c#
using XKit.Lib.Common.Registration;

namespace Tutorial.Session
{
    public static class Constants
    {
        public static readonly Descriptor ServiceDescriptor = new Descriptor {
            Collection = "Tutorial",
            Name = "Session",
            Version = 1
        };
    }
}
```

Finally, build Tutorial.Common again to make sure it builds.

```
#!bash
:~/Tutorial.Session.Common$ dotnet build 
```

NOTE:  If you are using VS Code as your editor, sometimes it can get confused with a bunch of changes.  If you see errors in VS Code, but the build is fine from the command line, then restart Omnisharp.  In VS Code, hit Ctrl-Shift-P and type "Restart Omnisharp" and click it. 

### Point the Tutorial.Session project at the new Tutorial.Session.Common

Now that we did all that, our original Tutorial.Session is broken, because it doesn't know where to find `ISessionApi.cs` or `Session.cs`.  Let's fix that. 

Edit the `Tutorial.Session.csproj` file and add the following to one of the `<ItemGroup>` sections:

```
#!XML
    <ProjectReference Include="../Tutorial.Session.Common/Tutorial.Session.Common.csproj"/>
```

And build it.  From the `Tutorial.Session` folder:  
```
#!bash
:~/Tutorial.Session.Common$ dotnet build 
```

And if everything happened as it should, it should build correctly.  

There's one more tiny detail.  Remember how we had to pass the "service descriptor" when we created the `Session` service?  In `Program.cs`:

```
#!c#
    host.AddCreateManagedService(
        serviceDescriptor: new Descriptor {
            Collection = "Tutorial",
            Name = "Session",
            Version = 1
        },
        typeof(ISessionApi)
    );
```

Well, we have that information in the `Tutorial.Session.Common` project now.  We should just reference that.  
```
#!c#
    host.AddCreateManagedService(
        serviceDescriptor: Constants.ServiceDescriptor,
        typeof(ISessionApi)
    );
```

We should be good to go with no errors.  If you see a bunch of weird errors, try restarting Omnisharp (see above) before panicking.

## Step 2:  Create the Client

### Create the project 

Now that we have that bit of housekeeping out of the way, let's create a _Client_ for our service.  The client will allow other C# code (other services or just any code) to access our `Session` service.  

We'll use a library project again.  From the parent folder that contains `Tutorial.Session` and `Tutorial.Session.Common`...

```
#!bash
:~/$ mkdir Tutorial.Session.Client
:~/$ cd Tutorial.Session.Client
:~/Tutorial.Session.Client$ dotnet new classlib
```

Edit the `Tutorial.Session.Client.csproj` file and add XerviceKit references, as well as a reference to our Tutorial.Session.Common project:

```
#!XML
  <ItemGroup>
    <PackageReference Include="XKit.Lib.Connector" Version="*"/>
    <PackageReference Include="XKit.Lib.Connector.Protocols.Direct" Version="*"/>
    <PackageReference Include="XKit.Lib.Connector.Protocols.Http" Version="*"/>
    <ProjectReference Include="../Tutorial.Session.Common/Tutorial.Session.Common.csproj"/>
  </ItemGroup>
```

### Create the Client class

In the same folder, rename the default file that dotnet placed there for us when we created the project.  Let's rename it to "Client.cs"

```
#!bash
:~/Tutorial.Session.Common$ mv Class1.cs Client.cs 
```

The Client class will just be a thin wrapper that makes service calls for us.  It will implement our ISessionApi interface as well. 

```
#!c#
using System;
using System.Threading.Tasks;
using XKit.Lib.Common.Client;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Registration;
using XKit.Lib.Connector.Service;

namespace Tutorial.Session
{
    public class Client : ServiceClientBase<ISessionApi>, ISessionApi
    {
        public Client()
            : base(Constants.ServiceDescriptor)
        {
        }

        public Task<ServiceApiResult> UpsertSession(Session request)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceApiResult> DeleteSession(Session request)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceApiResult<Session>> GetSession(Session request)
        {
            throw new NotImplementedException();
        }
    }
}
```

We'll fill in some code in a minute.  For now, make sure the project builds (use dotnet build).

### Add code to the Client class

Now let's add in a tiny bit of wiring to let this class talk to the service.  Modifying the `UpsertSession` and `GetSession` methods in the `Client` class...

```
#!c#
    public Task<ServiceApiResult> UpsertSession(Session request)
    {
        // ----->
        return this.ExecuteCall(request);
        // <-----
    }
    ...
    public Task<ServiceApiResult<Session>> GetSession(Session request)
    {
        // ----->
        return this.ExecuteCall<Session, Session>(request);
        // <-----
    }

```
_What just happened?_  

In both cases, we call the `ExecuteCall` method from the base class, and it does all the work of finding the service and calling it's api.  How does it know what method to call?  It looks at it's own containing method (in the case of upserting the user the method is called obviously `UpsertSession` and from that it knows the name of the service api method to call.   

(Side note:  In the case of `UpsertSessions`, the compiler could figure out what types are needed for the generic `ExecuteCall`.  But in the case of `GetSessions`, the compiler can't figure it all out on it's own, so we specify `<Session, Session>`.  That's just saying that we expect to use a request object of `Session` and we expect a response also of type `Session`.)

Build the project one more time, and that's it.  We will use this project a bit later when we do the test. 

## Step 3:  Create the Test

### Create the project 

Good automated tests are a foundation cornerstone of maintainable software.  Thankfully, they are easy to write.  Let's create a test project for our service and add some tests to it.  The test will use the _client_ that we just created.

A quick note about testing:  There are different philosophies about the user of _Unit_ tests versus _Integration_ or _System_ tests versus _Functional_ tests.  Any are possible using XerviceKit.  In this case, we'll use a kind of _Unit_ test approach where the "units" in question are the service operation methods.  Really though, as you will see, it is possible to do a kind of hybrid test that allows for less complicated tests at the same time as combining some strengths of _Unit_ and _Integration_ tests.  This will be discussed in a later tutorial.

Lets create a test project.  From the parent folder that contains `Tutorial.Session`...

```
#!bash
:~/$ mkdir Tutorial.Session.Test
:~/$ cd Tutorial.Session.Test
:~/Tutorial.Session.Test$ dotnet new mstest
```

Edit the `Tutorial.Session.Test.csproj` file and add XerviceKit references, as well as a references to our other projects:

```
#!XML
  <ItemGroup>
    <PackageReference Include="XKit.Lib.Testing" Version="*"/>
    <ProjectReference Include="../Tutorial.Session.Common/Tutorial.Session.Common.csproj"/>
    <ProjectReference Include="../Tutorial.Session.Client/Tutorial.Session.Client.csproj"/>
    <ProjectReference Include="../Tutorial.Session/Tutorial.Session.csproj"/>
  </ItemGroup>
```

### Create the Test class

In the same folder, rename the default file that dotnet placed there for us when we created the project.  Let's rename it to "UpsertSessionTests.cs".  This class will contain all the tests for `UpsertSession`.

```
#!bash
:~/Tutorial.Session.Test$ mv UnitTest1.cs UpsertSessionTests.cs 
```

Let's start with the basic layout of the class. 

```
#!c#
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XKit.Lib.Testing.Integration;

namespace Tutorial.Session.Test
{
    [TestClass]
    public class UpsertSessionTests
    {
    }
}
```

Now let's add some code to so that this test class can test our service api method.

### Add class init and cleanup 

TODO: 
Add the following two methods to the test class:
```
#!c#
    [ClassInitialize]
    public static void ClassInit(TestContext context) {
        TestHostHelper.Initialize();
        TestHostHelper.AddCreateService(
            Constants.ServiceDescriptor,
            typeof(ApiOperation)
        );
        TestHostHelper.StartHost();
    }

    [ClassCleanup]
    public static void ClassTeardown() {
        TestHostHelper.DestroyHost();
    }
```

_What just happened?_ 

The tests will occur in a "real" service environment.  So the test helper is doing some behind the scenes magic to initialize our host, add our service, and start it.  It is also destroying it appropriately at the end of all the tests.   

Now we are ready to add the a test.  Let's start with one that checks to see if `UpsertSession` correctly creates a new user.  

```
#!c#
    [TestMethod]
    public async Task UpsertWithNewSessionSucceeds()
    {
        var client = new Client();

        var upsertResult = await client.UpsertSession(new Session { 
            Sessionname = "kermit",
            Email = "kermit@thefrog.com",
            FullName = "Kermit TheFrog"
        });

        Assert.IsFalse(upsertResult.HasError);

        var getResult = await client.GetSession(new Session {
            Sessionname = "kermit"
        });

        Assert.AreEqual(getResult.ResponseBody.Sessionname, "kermit");
        Assert.AreEqual(getResult.ResponseBody.Email, "kermit@thefrog.com");
        Assert.AreEqual(getResult.ResponseBody.FullName, "Kermit TheFrog");
    }
```

_What just happened?_ 

1) We create a _client_ that lets us talk to the service.  The service is already up and running because of the `ClassInit` code. 

2) We call the service api method `UpsertSession` using the `client`.  Magic happens.

3) We check the result to see if there was an error.  So far so good!

4) We use the `client` again to call `GetSessions` to get the use we just created. 

5) We verify that the returned user has the data we expect. 

Build the project and run the test.  It should pass!

Let's add one more before we finish up. 

```
#!c#
    [TestMethod]
    public async Task UpsertWithExistingSessionSucceeds()
    {
        var client = new Client();

        var upsertResult1 = await client.UpsertSession(new Session { 
            Sessionname = "kermit",
            Email = "kermit@thefrog.com",
            FullName = "Kermit TheFrog"
        });

        var upsertResult2 = await client.UpsertSession(new Session { 
            Sessionname = "kermit",
            Email = "kermie@frogger.com",
            FullName = "Kermit D Frog"
        });
        Assert.IsFalse(upsertResult2.HasError);

        var getResult = await client.GetSession(new Session {
            Sessionname = "kermit"
        });

        Assert.AreEqual(getResult.ResponseBody.Sessionname, "kermit");
        Assert.AreEqual(getResult.ResponseBody.Email, "kermie@frogger.com");
        Assert.AreEqual(getResult.ResponseBody.FullName, "Kermit D Frog");
    }
```

_What just happened?_  

1) We create a _client_ again that lets us talk to the service.  

2) We call the service api method `UpsertSession` using the `client` to insert the user.

3) We check the result to see if there was an error.  

4) We call the service api method `UpsertSession` again to with different data to update the user.

5) We use the `client` again to call `GetSessions` to get the use we just updated. 

6) We verify that the returned user has updated the data. 

There is a small problem with both of these tests.  Maybe you noticed it?  It is possible if tests are run more than once, or if one is run after the other, that existing data might be present from a previous test run.  To fix this, we would want to clear any existing data at the start of each test.  For now though, we will leave it. 

## Your mission, should you choose to accept it...

* Using the same pattern as we started above, implement the method `DeleteSessions` in `Client` class.
* Implement tests for `GetSessions` and `DeleteSessions`.  Of course, you will have to think of test cases for each of them.
* Fix the problem mentioned in the tests where data can be left over.  It's actually quite easy.  See if you can figure out how to do it.

The project in this folder contains the final version.

## Final thoughts

At this point, the service we have written is now usable.  It runs, and code can talk to it.  It has been proved to work via tests.  

This tutorial had a lot of boring boilerplate stuff again, like creating projects, etc.    The main points to take away are in the guts of Step 2 where we create the Client and in Step 3 where we create tests.  

[Go to the next tutorial:  TBD](../Tutorial002_Organize_And_Add_Client_And_Tests/)



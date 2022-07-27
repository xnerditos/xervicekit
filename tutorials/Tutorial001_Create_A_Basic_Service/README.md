# Tutorial 1:  Create a basic service

[TOC]

## Description and objectives

This tutorial will:

* Walk you through creating the basics of creating a service project.  
* Walk you through coding one service API method. 
* Test the api method from the command line using `curl`
* Set you loose to create more service methods. 

The service will eventually have CRUD functions to manage users.  We will have `UpsertUser`, `GetUser`, and `DeleteUser` methods.  We'll just use file based JSON storage to keep things simple.

A note before beginning:  Most of what we do here is "boilerplate".  It is _not_ stuff you will need to worry about each time.  In fact, you can use a template to create the boring parts.  The interesting parts are only steps 3 and 4.  But we are going through this so you can get a peek behind the curtain and understand how things work. 

## Folder structure

Eventually we should have a folder structure that looks like this:
```
(root/home)
|
└───Tutorial.User
        ApiOperation.cs
        IUserApi.cs
        Program.cs
        Tutorial.User.csproj
        User.cs
```

## Assumptions

* This tutorial will assume that you are using the Linux command line (also available in Windows!)
* You will need the .NET Core framework 6 SDK installed.
* The recommended editor is Visual Studio Code, but you can also use a different one such as Visual Studio (full version).

## Prerequisites

* none

## Concepts

These are the concepts that you should understand as you begin this tutorial:

* A _Host_ is a process that "contains" _service_.  It manages all protocols that the services will use, starts them up, shuts them down, and generally manages them. 
* A _Service_ is a worker managed by a _Host_, a worker that implements some functionality.
* An _Operation_ is short-lived (stateless) object that does some work in the context of the service. 

## Step 1:  Create the basic project

First we need to create the dotnet core project.  We'll use a web project, because XerviceKit leverages the native .NET Kestrel engine for service calls over HTTP.

```
:~/$ mkdir Tutorial.User
:~/$ cd Tutorial.User
:~/Tutorial.User$ dotnet new web
```

You should get some output indicating that the project was created. 

## Step 2:  Wire up the host 

### Modify the project 

We need to add references to the XerviceKit nuget packages so that we can use them.

Open the `Tutorial.User.csproj` file and add the following:

```
  <ItemGroup>
    <PackageReference Include="XKit.Lib.Host" Version="*"/>
    <PackageReference Include="XKit.Lib.Host.Helpers" Version="*"/>
    <PackageReference Include="XKit.Lib.Host.Protocols.Http" Version="*"/>
  </ItemGroup>
```

### Modify `Program.cs`

Now we just have to do a bit of wiring to get the host operational.

Make the `Program.cs` look like this:
```
using Tutorial.User;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;
using XKit.Lib.Host;
using XKit.Lib.Host.Protocols.Http;

var builder = WebApplication.CreateBuilder(args);

HostEnvironmentHelper hostHelper = new();
var host = hostHelper.CreateInitHost(hostAddress: "localhost");

// TODO:  Add in XerviceKit service here

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

_What just happened?_  

* The line `var host = hostHelper.CreateInitHost(hostAddress: "localhost");` creates a host object and init's it.  
* The line `hostHelper.StartHost();` starts the host running.  We will be coming back to this later, because we will need to add our service in a minute.
* The line `builder.Services.AddMvcCore().AddXKitHostMvc();` adds in framework pieces that are needed for requests to get routed.  XerviceKit uses MVC to route HTTP based requests, so we need to hook in to the pipeline.
* The line `services.AddScoped(...` injects the Xervicekit host environment as a dependency so that it can be found when requests come in.

## Step 3:  Create the service api interface and service api operation class

Pretty much everything up until now as been boring "boilerplate" code to hook XerviceKit in to the process and set up the environment.  The basics of all of that won't really change much.  But now, we are getting to the interesting parts. 

### The Service Api interface

A "service api" is the set of methods that other services or consumers can call.  These are defined by an "api interface".  In a minute we'll create one. 

First, we need a "User" entity (POCO) to pass around.  

Create a new file in your project called `User.cs`.  The User poco just has some basic user information. 

```
namespace Tutorial.User; 
public class User {
    public string Username { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public string Password { get; set; }
}
```

Ok, now the api interface:  Create a new file in your project called `IUserApi.cs`.  

```
using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;

namespace Tutorial.User;

public interface IUserApi : IServiceApi
{
    Task<ServiceCallResult> UpsertUser(User request);
}
```

_Ok, what just happened?_  


The service api interface must derive from `IServiceApi` so that XerviceKit knows what it is.  The `UpsertUser` method has an interesting signature.  It is `async`, and returns a `ServiceCallResult`.  `ServiceCallResult` is a general result object that a service api method uses to communicate what happened when it ran.  In this case, the result does not include any specific response object, it's kind of like a `void` method.  It only returns a general object to indicate operational results.  

`UpsertUser` takes a `User` object as it's request.  Service Api methods optionally can have a request object as a parameter.  Our request for `UpsertUser` will tell the method the information about the user being created.

### The Operation class

An interface without any class that uses it doesn't help us at all.  Let's fix that now.  We will add the "operation" class that does the actual work. 

Add a file to your project called `ApiOperation.cs`.  Make the class derive from `ServiceOperation` and make it inherit the interface we just created `IUserApi`.

```
using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Host.DefaultBaseClasses;

namespace Tutorial.User

public class ApiOperation : ServiceApiOperation, IUserApi
{
    public ApiOperation(ServiceApiOperationContext context) 
        : base(context)
    {
    }

    public Task<ServiceCallResult> UpsertUser(User request)
    {
        throw new System.NotImplementedException();
    }
}
```

_What just happened?_  

We added a method that fulfilled the interface.  So at this point the code is compilable, even though obviously it's not really doing anything yet. 

## Step 4:  Add some code to the api operation

There is a big, ugly `NotImplementedException` sitting in the middle of our beautiful new api method.  We can't have that hanging around.  Let's make an implementation.  

First of all, something to understand:  The base class `ServiceOperation` provides all kind of default functionality in the background for us.  We want to use that free functionality.  So we'll do this implementation in two parts. 

### Create a method with the code we want to run. 

Add a new private method that will do the "upserting" of a user by simply writing out the data as `json`.  

Add these usings.
```
using Newtonsoft.Json;
using System.IO;
```

Now add the method
```
private async Task DoUpsertUser(User request) 
{   
    var jsonString = JsonConvert.SerializeObject(request);
    var path = $"{HostEnvironment.DataRootFolderPath}/{request.Username}.json";
    await File.WriteAllTextAsync(path, jsonString);
}
```

### Tell the operation to call DoUpsertUser when the api method is called

Replace the body of the `UpsertUser` method:
```
public Task<ServiceCallResult> UpsertUser(User request)
{
    return RunApiOperation(
        request,
        operationAction: DoUpsertUser
    );
}
```

_What just happened?_  

We are calling the base method `RunOperation` to invoke our method.  `RunOperation` will perform all kinds of magic for us.  It will take care of creating a log session, of figuring out threading concerns, of composing our `ServiceCallResult`, etc.  The point is, it will all just work and `DoUpsertUser` will get executed just like it should.  

## Step 5:  Create the service and add it to the host

Ok, we're almost ready to run this sucker.  One more thing we have to do.  Remember when we created the host and started it?  Well, we need an actual _service_ object that will create an operation when a request comes in and will hand the operation the execution.  Remember, _operations_ are stateless.  But they are associated with a _service_ which is managed by the _host_.

It's super easy to create the service.  In `Program.cs`, add the following line where the `TODO` is currently located: 

```
host.AddCreateManagedService(
    serviceDescriptor: new Descriptor {
        Collection = "Tutorial",
        Name = "User",
        Version = 1
    },
    typeof(ApiOperation)
);
```

_What just happened?_  

`AddCreateManagedService` will create a _service_ for us to use.  The `Descriptor` object tells the host the name of the new service so that the api can be called. It also tells the host that `IUserApi` is the associated api interface.  The host will be able to automatically find our _operation_ object and execute the `UpsertUser` method when something calls it. 

## Step 6:  Test the service

This thing should be ready to run.  Let's try it.  

1) Put a breakpoint in your code at the start of `DoUpsertUser`.  

2) Start the project in the debugger.  

3) Run the following command (you will have to use the correct port if it's not 5000).

```
$> curl -d '{"operationName":"UpsertUser","payloadObj":{"username":"kermit","email":"kermit@frogville.com","fullName":"Kermit TheFrog","password":"ribbot!"}}' -H "Content-Type: application/json" -X POST http://localhost:5000/managed/Tutorial/User/1
```
... OR ...

Use Postman or some such tool to do the same thing.
`POST` to the url `http://localhost:5000/managed/Tutorial/User/1` (be careful about using the right port!) using this post body: 

```
{
    "operationName": "UpsertUser",
    "payload": {
        "username": "kermit",
        "email": "kermit@frogville.com",
        "fullName": "Kermit TheFrog",
        "password": "ribbot!"
    }
}
```

(Note that the bash script `test-create-user.sh` has been placed in the associated `Tutorial.User` folder here in this repo for your enjoyment.  It contains the `curl` command indicated.)

In either case, if you are running in the debugger and you have a breakpoint at `DoUpsertUser`, your debugger should stop at the breakpoint.  

If you let it run, you should get back a response something like this: 
```
{
    "responseBody": null,
    "serviceCallStatus": 1,
    "operationStatus": 1,
    "serviceStatus": {
        "instanceId": "835511b8c88a46f2ad6b453933670a07",
        "availability": 5,
        "health": 4,
        "runState": 3
    },
    "message": null,
    "responderInstanceId": "835511b8c88a46f2ad6b453933670a07",
    "responderFabricId": "361e06ed43fe4ac6b33e5cff614b8243",
    "operationId": "fe267c70-fcd6-47ee-b2f7-fd709538987c",
    "operationName": "UpsertUser",
    "service": {
        "isMetaService": false,
        "collection": "Tutorial",
        "name": "User",
        "version": 1,
        "updateLevel": 0,
        "patchLevel": 0
    },
    "correlationId": "0394b857f4214788988bf38bb871d31b",
    "requestorInstanceId": null,
    "requestorFabricId": null,
    "correlationTags": null,
    "passThru": null,
    "code": null,
    "hasError": false,
    "immediateSuccess": true,
    "completed": true
}
```

The service api was called!

## Your mission, should you choose to accept it...

* Using the same pattern as we started above, create a service api method for `DeleteUser`.  
    * The `DeleteUser` call should simply delete the json file associated with the user.
* Implement the `GetUsers` method.  
    * The signature in the `IUserApi` will be `Task<ServiceCallResult<User>> GetUser(User request)`.  
    * For the `DoGetUser` method, the signature will be `Task<User> GetUser(User request)`.  
    * In the `DoGetUser` method, use `request.Username` to determine the name of the file with the json data, and return a fully hydrated `User` object with all the information.

The project in this folder contains the final version.

## Final thoughts

Although most of this was boilerplate code, you should hopefully have in your mind certain concepts starting to take form.  The most interesting part of this exercise was the operation logic, that is, the `DoUpsertUser` part. 

Why do we say that?  Because all the boring parts XerviceKit is doing.  It is handling the communications protocols for example.  In a few more tutorials we will see how it handles automatic discovery of services.  And a bunch more goodness we can't wait to show you.  

[Go to the next tutorial:  Organize and add a client and tests](../Tutorial002_Organize_And_Add_Client_And_Tests/)

# Setting up a Service

This page will take you step by step through setting up an XerviceKit service project.  It will follow the steps to set up a "typical" project, with some side links for special case situations. 

Before setting up, we recommend having [set up a host](./GettingStarted-Hosts.md) first, since part of these instructions will be to add the service to the host. 

## Overview

We recommend you set up your project with the following sub-projects. This is not strictly necessary, but it makes for better organization and allows clients to avoid unnecessary library dependencies.

* Common:  A project to contain stuff that is needed on both the client and service side.  The service API interface for example would go here. 
* Client:  Provides objects to access the service and make calls
* Service: The code for the service operation and service itself
* Tests:  Code that tests the service methods.

Note that most of the code in these projects is "boilerplate" and you could make a template to quickly set up a service. 

In fact, a [sample service](../samples/SampleService/) is available that you can use as a starting point. 

## Set up the individual projects

### Host  

Remember, a [host](BasicConcepts.md#hosts) is only needed when you are going to actually _use_ and deploy a service.  More than one service can be run in a host.  So if you have several services here and repeat this process for each of them, you only need to create as many hosts as you will actually use instead of creating one per service. 

#### Create the project 

```
:~/$ mkdir Host
:~/$ cd Host
:~/Host$ dotnet new web
```

We use `dotnet new web` because we are assuming you will be using the default protocols, which use ASP.NET for HTTP request processing. 

#### Add the following references to your Host project:

```
<PackageReference Include="XKit.Lib.Host" Version="*"/>
<PackageReference Include="XKit.Lib.Host.Helpers" Version="*"/>
<PackageReference Include="XKit.Lib.Host.Protocols.Http" Version="*"/>
```

Obviously the version should be whatever is the latest XerviceKit version.  `*` can be used but it is recommended that you peg the dependencies to a specific version for projects that are going into production. 

#### Make your `Program.cs` look something like this: 

```
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using XKit.Lib.Common.Registration;
using XKit.Lib.Host;


namespace Host1;

public class Program
{
    private static readonly HostEnvironmentHelper hostHelper = new(); // <---

    public static void Main(string[] args)
    {
        var host = hostHelper.CreateInitHost(hostAddress: "localhost"); // <---

        // TODO:  TBD, add services to the host

        hostHelper.StartHost();  // <---
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            })
            .ConfigureServices(services => {
                services.AddScoped<XKit.Lib.Common.Host.IXKitHostEnvironment>(_ => hostHelper.Host);  // <---
            });
}
``` 

The significant lines are marked with `// <---` above. 

#### Make your `Startup.cs` look something like this: 

```
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using XKit.Lib.Host.Protocols.Http;

namespace Host1; 

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddMvcCore()
            .AddXKitHostMvc();  // <---
        services
            .AddControllers();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
```

The above is all boilerplate and you could convert it into a template for re-use.  Again, the significant lines are marked with `// <---`

### Common

Common has all of the common stuff used on both the client and service side.  This includes the service api interface, request and response objects, event and command interfaces, etc.  

Assume that `ServiceName` below refers to the name of your service. 

#### Create the project 

```
:~/$ mkdir ServiceName.Common
:~/$ cd ServiceName.Common
:~/ServiceName.Common$ dotnet new classlib
```

Add the following reference to your csproj file:
```
<PackageReference Include="XKit.Lib.Common" Version="*"/>
```


#### Add a `Constants.cs` file where we can put common stuff.  

The file should look something like this.  Replace `ServiceCollection` with the collection (organizational division or category of service) and `ServiceName` with the name of the service. 

```
using XKit.Lib.Common.Registration;

namespace ServiceName.Common;

public static class Constants
{
    public static readonly Descriptor ServiceDescriptor = new Descriptor {
        Collection = "ServiceCollection",
        Name = "ServiceName",
        Version = 1
    };
}

```

#### If the service has an API (most services do) then add the API interface.  

It will look something like this:  

```
using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;

namespace ServiceName.Common;

public interface IServiceNameApi : IServiceApi
{
    Task<ServiceCallResult<Method1Result>> Method1(Method1Request request);
}
```

Define the methods of the service API.  For each request or result object add a POCO class to the Common library.  (ie, you need to define a class for `Method1Result` and `Method1Request` in the above example)

### Client

no Data

### Service

Ooohhh we are getting to the good stuff in this section!  

Assume that `ServiceName` below refers to the name of your service. 

#### Create the project

```
:~/$ mkdir ServiceName
:~/$ cd ServiceName
:~/ServiceName$ dotnet new classlib
```

#### Add the following references to the csproj file

```
<PackageReference Include="XKit.Lib.Host" Version="*"/>
<PackageReference Include="XKit.Lib.Host.Helpers" Version="*"/>
<PackageReference Include="XKit.Lib.Host.Protocols.Http" Version="*"/>
<ProjectReference Include="../ServiceName.Common/ServiceName.Common.csproj"/>
```

Again, obviously the same point applies about versions.  Also, notice we are making a project reference to our Common project. 

#### Add the service operation code

The service operation code is where the majority of the "work" happens.  Make your service operation look something like this:

```
using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Host.DefaultBaseClasses;
using System.IO;

namespace ServiceName;

public class ApiOperation : ServiceOperation, IServiceNameApi
{
    public ApiOperation(ServiceOperationContext context) 
        : base(context)
    {
    }

    public Task<ServiceCallResult<Method1Result>> IServiceNameApi.Method1(Method1Request request)
    {
        return RunServiceCall(
            request,
            operationAction: DoMethod1
        );
    }

    // ---------------------------------------------------------------------

    private async Task<Method1Result> DoMethod1(Method1Request request) 
    {   
        // TODO:  Do something interesting
    }
}
```

Some points:
* The name of the class can be whatever you want.  But by convention it should contain the word "Operation".
* Obviously, repeat the pattern for `Method1` with your own methods that you want to define.
        await File.WriteAllTextAsync(path, jsonString);
* It is recommended that you use explicitly defined methods for implementing the service API interface. 
* `RunServiceCall()` is capable of also taking a few other methods to do things like validate the request. 

#### Define the service code

In most cases, you do not need to define anything special for the service code itself.  Everything interesting is in the Operation.  If this is the case, you don't need to do anything for defining service specific code. 

However, there are some cases whereby you need to define a specific class for the service.  

* If the service has dependencies (it calls other services), then you will need to define a service class.
* if an operation requires access to special long-lived resources (like maybe database connections), sometimes these are managed by the main service code.  If this is the case, and the Operation will obtain these from the Service, then you need to define a service class. 
* If there is some special functionality that is best placed at the level of the service itself, you will derive a service class.  

In such a cases, [this page will explain how to define the service code](./GettingStarted-ServiceCode.md). 

#### Add the service to the host

You need to add the service to the host so that the host will know about it.  This can be repeated for as many services as you wish to add. 

In the host csproj file, add a reference to the common project and the service project: 

```
<ProjectReference Include="../ServiceName.Common/ServiceName.Common.csproj"/>
<ProjectReference Include="../ServiceName/ServiceName.csproj"/>
```

Then add the service to the host.  If you did _not_ create a service class and only have an Operation, then add this line to the host _before_ the call to `StartHost()`

```
host.AddCreateManagedService(
    ServiceName.Common.Constants.ServiceDescriptor, 
    typeof(ApiOperation)
);
```

If you _did_ create a service class, then the call is slightly different: 

```
host.AddManagedService(new ServiceName.ServiceNameService(host));
```

## What next?

Your service is now created.  You'll no doubt want to add code to it.  But it is also important to add appropriate testing along with your code.  The next step is to [add tests for your API methods](./GettingStarted-TestingServiceApi.md) to ensure your code works the way it should. 


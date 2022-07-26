# Setting up a Host

This page will take you step by step through setting up a XerviceKit Host project.  

Remember, a [Host](BasicConcepts.md#hosts) is where the services will actually run.  Hosts and services do not have a 1 to 1 relationship (one host can run many services, and a service can run in more than one host), so exactly how many hosts you create is up to you.  If you are just starting out, we recommend taking a "monolith" approach in which you host all your services together in one.  

A "monolith" architecture simplifies things when you are in a learning phase.  A multi-host architecture will require that you also have a Registry service running in one of the hosts as well as the use of host addresses.  

You move to a multi-host architecture later by simply creating more than one host and then taking the steps mentioned.  See the following point for more information.

## Before setting up the project

If you will have more than one host, then you will need to define the following environment variables on the machine (or in the container) where each one will run:  

* `HOST_BASE_ADDRESS`:  This is the "address" that other actors use to reach the host.  It will usually be a DNS name that resolves to the host plus optionally a port.  Examples:  `192.168.1.32:5000`, `host1.local`, `host2.local:5000`
* `INITIAL_REGISTRY_ADDRESSES`:  at list one address (in the same format as `HOST_BASE_ADDRESS`) of the host where the Registry is located. 

Note that if you will be running a monolith (all services in one host) you do not need to define the above environment variables.  

## Set up the project

This [TBD: sample host]() is available that you can use as a starting point as well. 

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
using Tutorial.User;

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

The interesting lines are marked with `// <---` above. 

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

The above is all boilerplate and you could convert it into a template for re-use.  Again, the interesting lines are marked with `// <---`

## What next?

If you are setting up for the first time, you now have a host to run your services.  The next step is to [create a service](./GettingStarted-Services.md) to use in your host. 

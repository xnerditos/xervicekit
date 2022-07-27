using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using XKit.Lib.Common.Registration;
using XKit.Lib.Host;
using Tutorial.User;
using Tutorial.Session;

namespace Tutorial.Host1;

public class Program
{
    private static readonly HostEnvironmentHelper hostHelper = new();

    public static void Main(string[] args)
    {
        var host = hostHelper.CreateInitHost(hostAddress: "localhost");
        host.AddCreateManagedService(
            serviceDescriptor: new Descriptor {
                Collection = "Tutorial",
                Name = "User",
                Version = 1
            },
            typeof(User.ApiOperation)
        );
        host.AddManagedService(new SessionService(host));
        hostHelper.StartHost();
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            })
            .ConfigureServices(services => {
                services.AddScoped<XKit.Lib.Common.Host.IXKitHostEnvironment>(_ => hostHelper.Host);
            });
}

using Tutorial.User;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;
using XKit.Lib.Host;
using XKit.Lib.Host.Protocols.Http;

var builder = WebApplication.CreateBuilder(args);

HostEnvironmentHelper hostHelper = new();
var host = hostHelper.CreateInitHost(hostAddress: "localhost");

host.AddCreateManagedService(
    serviceDescriptor: new Descriptor {
        Collection = "Tutorial",
        Name = "User",
        Version = 1
    },
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

using XKit.Lib.Common.Host;
using XKit.Lib.Host;
using XKit.Lib.Host.Protocols.Http;

var builder = WebApplication.CreateBuilder(args);

HostEnvironmentHelper hostHelper = new();
var host = hostHelper.CreateInitHost(hostAddress: "localhost");

// TODO:  Add in XerviceKit services here
// host.AddCreateManagedService();

builder.Services.AddScoped<IXKitHostEnvironment>(_ => hostHelper.Host);
builder.Services.AddMvcCore().AddXKitHostMvc();
builder.Services.AddControllers();

var app = builder.Build();
app.UseRouting();
app.UseEndpoints(endpoints => endpoints.MapControllers());

hostHelper.StartHost();
app.Run();
hostHelper.StopAndDestroyHost();

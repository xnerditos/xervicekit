using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using XKit.Lib.Host.Protocols.Http.Mvc;

namespace SystemTests.ServiceCalls.TestsRemote; 

public class AspStartup {

    public AspStartup(IConfiguration configuration) {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services) {

        services.AddControllers();
        services
            .AddMvcCore()
            .AddXKitHostMvc();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment _) {

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
        app.UseDeveloperExceptionPage();
    }
}

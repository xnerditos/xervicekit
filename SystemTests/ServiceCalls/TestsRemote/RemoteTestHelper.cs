using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using XKit.Lib.Host.Protocols.Http.Mvc;

namespace SystemTests.ServiceCalls.TestsRemote {

    public class RemoteTestHelper {
        private static bool isInited = false;

        public static void InitAsp() {
            if (!isInited) {
                WebHost
                    .CreateDefaultBuilder()
                    //.UseUrls($"http://{HostEnvironment.LocalAddress};https://0.0.0.0:8091")
                    //.UseUrls($"http://{HostEnvironment.LocalAddress}")
                    .UseKestrel(options => {                    
                        options.Listen(IPAddress.Any, 8080); 
                    })
                    .UseStartup<RemoteTestHelper>();
                isInited = true;
            }
        }

        public RemoteTestHelper(IConfiguration configuration) {
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
}

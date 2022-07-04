using XKit.Lib.Common.Host;
using XKit.Lib.Testing;
using Microsoft.AspNetCore;
using XKit.Lib.Common.Utility.Extensions;
using System.Threading;
using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace SystemTests.ServiceCalls.TestsRemote; 

public class RemoteTestBase {

    private static TestHostHelper testHelper = null;
    protected static TestHostHelper TestHelper => testHelper;

    public void InitializeCommon() { 
        if (testHelper == null) {
            testHelper = new TestHostHelper();
            testHelper.InitializeRemoteTestHost();
            InitAsp(testHelper.HostEnvironment);
        }
    }

    private static void InitAsp(IXkitHostEnvironment host) {
        var kestrelTask = WebHost
            .CreateDefaultBuilder()                    
            .UseKestrel(options => {                    
                options.Limits.MaxRequestBodySize = 100_000_000;
                options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(5);
                options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(5);
                options.ListenLocalhost(8090); 
            })
            .UseStartup<AspStartup>()
            .ConfigureServices(services => {
                services.AddScoped(_ => host);
            })
            .Build()
            .RunAsync();
        kestrelTask.Forget();
        Thread.Sleep(300);
    }
}

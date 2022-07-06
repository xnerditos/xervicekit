using XKit.Lib.Common.Host;
using XKit.Lib.Common.Services;

namespace SystemTests.Daemons {
    public interface ITestServiceFactory : IServiceFactory {
		IManagedService Create(
            IXKitHostEnvironment hostEnv
        );
    }
}

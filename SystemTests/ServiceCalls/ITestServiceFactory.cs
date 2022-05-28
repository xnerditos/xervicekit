using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Services;

namespace SystemTests.ServiceCalls {
    public interface ITestServiceFactory : IServiceFactory {

		IManagedService Create(
            ILocalEnvironment localEnvironment = null
        );
    }
}

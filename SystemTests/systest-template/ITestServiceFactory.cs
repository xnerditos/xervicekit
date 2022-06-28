using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Services;

namespace SystemTests._NAMESPACE {
    public interface ITestServiceFactory : IServiceFactory {

		IManagedService Create(
            ILocalEnvironment localEnvironment
        );
    }
}

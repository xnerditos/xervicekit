using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;

namespace XKit.Lib.Common.Client {
    public interface IInstanceClientFactory {

        void InitializeFactory(
            ILocalEnvironment localFabric = null
        );

		IInstanceClient TryCreateClient(
            IReadOnlyServiceInstance target
        );
    }
}

using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;

namespace XKit.Lib.Common.Client {
    public interface IInstanceClientFactory {

        void InitializeFactory(
            IXkitEnvironment localFabric = null
        );

		IInstanceClient TryCreateClient(
            IReadOnlyServiceInstance target
        );
    }
}

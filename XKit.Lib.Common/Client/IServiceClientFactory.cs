using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;

namespace XKit.Lib.Common.Client {

    public interface IServiceClientFactory {

        IReadOnlyDescriptor Descriptor { get; }
    }

    public interface IServiceClientFactory<TServiceClientInterface> : IServiceClientFactory {

        TServiceClientInterface CreateServiceClient(
            ILogSession log,
            IDependencyConnector connector,
            ServiceCallTypeParameters defaultCallTypeParameters = null
        );
    }
}

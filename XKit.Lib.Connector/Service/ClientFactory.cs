using XKit.Lib.Common.Client;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;

namespace XKit.Lib.Connector.Service {

    public interface IClientFactory {
        IGenericServiceClient CreateGenericServiceClient(
            IReadOnlyDescriptor descriptor,
            string operationInterfaceName,
            ILogSession log,
            IFabricConnector connector,
            ServiceCallTypeParameters defaultCallTypeParameters = null,
            ServiceClientErrorHandling errorHandling = ServiceClientErrorHandling.LogWarning,
            string targetHostId = null
        );
        IGenericServiceClient CreateGenericServiceClient<TCallInterface>(
            IReadOnlyDescriptor descriptor,
            ILogSession log,
            IFabricConnector connector,
            ServiceCallTypeParameters defaultCallTypeParameters = null,
            ServiceClientErrorHandling errorHandling = ServiceClientErrorHandling.LogWarning,
            string targetHostId = null
        ) where TCallInterface : IServiceCallable;

        ICommandMessenger<TCommandInterface> CreateCommandMessengerClient<TCommandInterface>(
            ILogSession log,
            IFabricConnector connector
        ) where TCommandInterface : IServiceCommands;

        IEventMessenger<TEventInterface> CreateEventMessengerClient<TEventInterface>(
            ILogSession log,
            IFabricConnector connector
        ) where TEventInterface : IServiceEvents;
    }

    public class ClientFactory : IClientFactory {

        private static IClientFactory factory = new ClientFactory();

        public static IClientFactory Factory => factory;

        // ===========================================================================
        // IClientFactory default implementations 
        // ===========================================================================

        ICommandMessenger<TCommandInterface> IClientFactory.CreateCommandMessengerClient<TCommandInterface>(
            ILogSession log,
            IFabricConnector connector
        ) => new CommandMessenger<TCommandInterface>(log, connector);

        IEventMessenger<TEventInterface> IClientFactory.CreateEventMessengerClient<TEventInterface>(
            ILogSession log,
            IFabricConnector connector
        ) => new EventMessenger<TEventInterface>(log, connector);

        IGenericServiceClient IClientFactory.CreateGenericServiceClient(
            IReadOnlyDescriptor descriptor, 
            string operationInterfaceName, 
            ILogSession log,
            IFabricConnector connector,
            ServiceCallTypeParameters defaultCallTypeParameters, 
            ServiceClientErrorHandling errorHandling, 
            string targetHostId
        ) => new GenericClient(
            descriptor: descriptor,
            log: log,
            operationInterfaceName: operationInterfaceName,
            defaultCallTypeParameters: defaultCallTypeParameters,
            connector: connector,
            errorHandling: errorHandling,
            targetHostId: targetHostId
        );

        IGenericServiceClient IClientFactory.CreateGenericServiceClient<TCallInterface>(
            IReadOnlyDescriptor descriptor, 
            ILogSession log,
            IFabricConnector connector,
            ServiceCallTypeParameters defaultCallTypeParameters, 
            ServiceClientErrorHandling errorHandling, 
            string targetHostId
        ) => new GenericClient(
            descriptor: descriptor,
            log: log,
            operationInterfaceName: typeof(TCallInterface).Name,
            defaultCallTypeParameters: defaultCallTypeParameters,
            connector: connector,
            errorHandling: errorHandling,
            targetHostId: targetHostId
        );
        
        // =====================================================================
        // Static methods
        // =====================================================================

        public static void InjectCustomFactory(
            IClientFactory factory
        ) => ClientFactory.factory = factory;
    }
}

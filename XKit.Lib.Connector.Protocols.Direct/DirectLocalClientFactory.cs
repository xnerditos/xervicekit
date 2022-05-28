using System.Collections.Generic;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Utility;
using XKit.Lib.Common.Client;
using XKit.Lib.Common.Utility.Extensions;

namespace XKit.Lib.Connector.Protocols.Direct {

    public interface IDirectLocalClientFactory : IInstanceClientFactory {

        void SetLoadParameters(
            bool autoDiscoverLocalServices = true
        );

        /// <summary>
        /// Adds a service to be available for direct access.  A service must be added
        /// for discover or else SetLoadParameters must be called with autoDiscoverLocalServices = true
        /// </summary>
        /// <param name="operationInterfaceType"></param>
        /// <param name="descriptor"></param>
        void AddServiceForDirectAccess(IManagedService service);
        void AddServiceForDirectAccess(IMetaService service);
    }

    public class DirectLocalClientFactory : IDirectLocalClientFactory {

        private static IDirectLocalClientFactory factory = new DirectLocalClientFactory();
        public static IDirectLocalClientFactory Factory => factory;

        private readonly Dictionary<string, IServiceBase> ServicesByIdentifier = new();
        private string LocalHostFabricId => localFabric.Value?.FabricId;
        private IHostEnvironment HostEnvironment => localFabric.Value?.HostEnvironment;
        private readonly SetOnceOrIgnore<ILocalEnvironment> localFabric = new();
        private bool autoDiscoverLocalServices = true;

        public DirectLocalClientFactory() { }

        // =============================================================================
        // IInstanceClientFactory
        // =============================================================================

        void IInstanceClientFactory.InitializeFactory(
            ILocalEnvironment localFabric
        ) {
            this.localFabric.Value = localFabric;
            ServicesByIdentifier.Clear();
        }

        IInstanceClient IInstanceClientFactory.TryCreateClient(
            IReadOnlyServiceInstance target
        ) {

            target = target ??
                throw new System.ArgumentException(null, nameof(target));

            if (HostEnvironment == null) {
                return null;
            }

            if (this.autoDiscoverLocalServices) {
                DiscoverServices();
                this.autoDiscoverLocalServices = false;
            }

            if (target.HostFabricId != null &&
                (target.HostFabricId.Equals(HostConstants.LocalHostFabricIdFlag) ||
                    target.HostFabricId.Equals(this.LocalHostFabricId))) {

                
                string identifier = Identifiers.GetServiceVersionLevelKey(target.Descriptor);
                ServicesByIdentifier.TryGetValue(identifier, out var service);

                if (service != null) {
                    return new DirectLocalClient(target, service);
                }
            }

            return null;
        }

        // =====================================================================
        // IDirectLocalClientFactory
        // =====================================================================

        void IDirectLocalClientFactory.SetLoadParameters(
            bool autoDiscoverLocalServices
        ) {
            this.autoDiscoverLocalServices = autoDiscoverLocalServices;
        }

        void IDirectLocalClientFactory.AddServiceForDirectAccess(
            IManagedService service
        ) {
            string identifier = Identifiers.GetServiceVersionLevelKey(service.Descriptor);
            ServicesByIdentifier[identifier] = service;            
        }

        void IDirectLocalClientFactory.AddServiceForDirectAccess(
            IMetaService service
        ) {
            string identifier = Identifiers.GetServiceVersionLevelKey(service.Descriptor);
            ServicesByIdentifier[identifier] = service;            
        }

        // =============================================================================
        // Private
        // =============================================================================

        private void DiscoverServices() {
            ServicesByIdentifier.Clear();

            HostEnvironment?
                .GetMetaServices()
                .ForEach(s => ServicesByIdentifier.Add(Identifiers.GetServiceVersionLevelKey(s.Descriptor), s));

            HostEnvironment?
                .GetManagedServices()
                .ForEach(s => ServicesByIdentifier.Add(Identifiers.GetServiceVersionLevelKey(s.Descriptor), s));
        }

        // =====================================================================
        // public static
        // =====================================================================

        public static void SetLoadParameters(
            bool autoDiscoverLocalServices
        ) => Factory.SetLoadParameters(
            autoDiscoverLocalServices
        );

        public static void AddService(
            IManagedService service        
        ) => Factory.AddServiceForDirectAccess(
            service
        );

        public static void AddService(
            IMetaService service        
        ) => Factory.AddServiceForDirectAccess(
            service
        );

        public static IInstanceClient TryCreateClient(
            IReadOnlyServiceInstance target
        ) => DirectLocalClientFactory.Factory.TryCreateClient(target);

        public static void InjectCustomFactory(IDirectLocalClientFactory factory) =>
            DirectLocalClientFactory.factory = factory;
    }
}

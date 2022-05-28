using System.Collections.Generic;
using XKit.Lib.Connector.Dependency;
using XKit.Lib.Common.Client;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Log;

namespace XKit.Lib.Connector.Fabric {

    public interface IFabricConnectorFactory {
        IFabricConnector Create(
            IList<IInstanceClientFactory> instanceClientFactories,
            IServiceCallRouterFactory serviceCallRouterFactory = null
        );     
    }

	public class FabricConnectorFactory : IFabricConnectorFactory {

        private static IFabricConnectorFactory factory = new FabricConnectorFactory();
        
        public static IFabricConnectorFactory Factory => factory;

		// ===========================================================================
  		// IFabricConnectorFactory default implementations 
  		// ===========================================================================

		IFabricConnector IFabricConnectorFactory.Create(
            IList<IInstanceClientFactory> instanceClientFactories,
            IServiceCallRouterFactory serviceCallRouterFactory
        ) {            
            return new FabricConnector(
                serviceCallRouterFactory ?? ServiceCallRouterFactory.Factory,
                instanceClientFactories ?? throw new System.ArgumentNullException(nameof(instanceClientFactories))
            );
		}

        // =====================================================================
        // Static methods
        // =====================================================================

		public static IFabricConnector Create(
            IList<IInstanceClientFactory> instanceClientFactories,
            IServiceCallRouterFactory serviceCallRouterFactory = null
        ) => Factory.Create(
            instanceClientFactories,
            serviceCallRouterFactory 
        );

        public static void InjectCustomFactory(
            IFabricConnectorFactory factory
        ) => FabricConnectorFactory.factory = factory;
	}
}

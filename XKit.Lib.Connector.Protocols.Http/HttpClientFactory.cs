using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Client;
using RestSharp;
using XKit.Lib.Common.Host;
using System;

namespace XKit.Lib.Connector.Protocols.Http {

	public class HttpClientFactory : IInstanceClientFactory {
		private static IInstanceClientFactory factory = new HttpClientFactory();
		public static IInstanceClientFactory Factory => factory;

		// ===========================================================================
  		// IInstanceClientFactory default implementations 
  		// ===========================================================================

        void IInstanceClientFactory.InitializeFactory(
            IXKitEnvironment localEnv
        ) { }

        [Obsolete]
		IInstanceClient IInstanceClientFactory.TryCreateClient(
			IReadOnlyServiceInstance target
		) {
			if (target == null) {
				throw new System.ArgumentException(null, nameof(target));
			}

			return new HttpClient(target, new RestClient("http://" + target.HostAddress));
			//return new HttpClient(target, new RestClient("https://jsonplaceholder.typicode.com"));
		}

        // =====================================================================
        // public static methods
        // =====================================================================

        public static IInstanceClient TryCreateClient(
            IReadOnlyServiceInstance target
        ) => Factory.TryCreateClient(target);

        public static void InjectCustomFactory(IInstanceClientFactory factory) =>
            HttpClientFactory.factory = factory; 
	}
}

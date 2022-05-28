using System.Collections.Generic;
using System.Threading.Tasks;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;

namespace XKit.Lib.Common.Fabric {

    /// <summary>
    /// Provides the ability to connect with the service fabric.
    /// </summary>
    public interface IFabricConnector : IDependencyConnector {

        /// <summary>
        /// Called to initialize the client 
        /// </summary>
        /// <returns>The fabric id assigned</returns>
        string Initialize();

        /// <summary>
        /// Registers as a consumer or host with the registry.   
        /// </summary>
        /// <param name="registration">entity describing the host</param>
        /// <param name="initialRegistryHostAddresses">Hosts that have the Registry service</param>
        /// <param name="localEnvironment">The host environment object that gives access to the current host</param>
        /// <returns>true if success</returns>
        Task<bool> Register(
            ILogSession log,
            IEnumerable<string> initialRegistryHostAddresses,
            ILocalEnvironment localEnvironment,
            bool failIfUnableToRegister = false
        );

        /// <summary>
        /// Updates the connector with the fabric.  If this is a host, then 
        /// this method updates the status of the host as well as getting
        /// refreshed dependency information.  If a host is not registered, then
        /// only dependency information is refreshed.
        /// </summary>
        /// <returns>true if succeeded</returns>
        Task<bool> Refresh(
            ILogSession log
        );

        /// <summary>
        /// Allows the consumer to forcibly set the dependency info 
        /// </summary>
        Task ForceResetTopologyMap(
            IReadOnlyServiceTopologyMap map
        );

        Task<bool> Unregister(ILogSession log);

        string FabricId { get; }

        bool IsHost { get; }
    }
}

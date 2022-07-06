using System.Collections.Generic;
using System.Threading.Tasks;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;

namespace XKit.Lib.Common.Fabric {

    /// <summary>
    /// Provides the ability to connect with the service fabric.
    /// </summary>
    public interface IFabricConnector {

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
        /// <param name="xkitEnvironment">The host environment object that gives access to the current host</param>
        /// <returns>true if success</returns>
        Task<bool> Register(
            ILogSession log,
            IEnumerable<string> initialRegistryHostAddresses,
            IXKitEnvironment xkitEnvironment,
            bool failIfUnableToRegister = false
        );

        /// <summary>
        /// Registers as a consumer or host with the registry.   
        /// </summary>
        /// <param name="registration">entity describing the host</param>
        /// <param name="initialRegistryHostAddresses">Hosts that have the Registry service</param>
        /// <param name="hostEnvironment">The host environment object that gives access to the current host</param>
        /// <returns>true if success</returns>
        Task<bool> Register(
            ILogSession log,
            IEnumerable<string> initialRegistryHostAddresses,
            IXKitHostEnvironment hostEnvironment,
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

        /// <summary>
        /// Gets an IServiceCallRouter capable of communicating with the target dependency
        /// </summary>
        /// <param name="target"></param>
        /// <param name="failIfNotAvailable"></param>
        /// <returns></returns>
        Task<IServiceCallRouter> CreateCallRouter(
            IReadOnlyDescriptor target, 
            ILogSession log,
            bool failIfNotAvailable = true,
            bool allowRegistryRefreshIfRequested = true
        );

        string FabricId { get; }

        bool IsHost { get; }
    }
}

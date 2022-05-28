using System.Collections.Generic;
using System.Threading.Tasks;
using XKit.Lib.Common.Client;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;

namespace XKit.Lib.Common.Fabric {

    /// <summary>
    /// Handles communication with service dependencies
    /// </summary>
    public interface IDependencyConnector {

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
    }
}

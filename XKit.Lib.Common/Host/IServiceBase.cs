using XKit.Lib.Common.Registration;
using System.Collections.Generic;
using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Services.MessageBroker;
using XKit.Lib.Common.Log;

namespace XKit.Lib.Common.Host {

    /// <summary>
    /// IServiceBase represents the common interface for all services (managed and meta)
    /// </summary>
    public interface IServiceBase {
        IReadOnlyDescriptor Descriptor { get; }
        IReadOnlyServiceCallPolicy CallPolicy { get; }
        IEnumerable<IReadOnlyDescriptor> Dependencies { get; }
        IEnumerable<IReadOnlySubscription> EventSubscriptions { get; }
        IEnumerable<IReadOnlySubscription> CommandSubscriptions { get; }
        IXKitHostEnvironment HostEnvironment { get; }
        
        /// <summary>
        /// The key to identify the service's config document
        /// </summary>
        /// <value></value>
        string ConfigurationDocumentIdentifier { get; }
        string InstanceId { get; }
		RunStateEnum ServiceState { get; }
        ServiceInstanceStatus GetServiceStatus();

        Task<ServiceCallResult> ExecuteCall(
            ServiceCallRequest request
        );

        bool CanStartNewOperation();
        bool HasFeature(string featureName);
        void SignalEnvironmentChange(ILogSession log);
        IServiceDaemon GetDaemon(string daemonName);
        IServiceDaemon[] GetDaemons();
        void StartService(ILogSession log);
        void StopService(ILogSession log);
        void SignalHostStartupComplete(ILogSession log);
        void SignalHostShutdownComplete(ILogSession log);
    }
}

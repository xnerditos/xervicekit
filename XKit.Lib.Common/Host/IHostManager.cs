using System.Collections.Generic;
using System.Threading.Tasks;
using XKit.Lib.Common.Registration;

namespace XKit.Lib.Common.Host {
    
    public interface IHostManager : IHostEnvironment, ILocalEnvironment {

		RunStateEnum HostState { get; }

        string LocalDataFolderPath { get; }

        string MetaDataDbPath { get; }

        /// <summary>
        /// Add a service to the host.
        /// Note that the service implementation being used must implement
        /// the IManagedService interface for the host to interact with it as well
        /// as a "core" interface that the operation will use. 
        /// </summary>
        /// <param name="service">The service implementing object</param>
        void AddManagedService(
            IManagedService service
        );

        /// <summary>
        /// Creates a "generic" service and adds it to the host.
        /// This is useful for simpler operations that do no require any long lived resources
        /// to be managed by the service core.
        /// </summary>
        /// <param name="serviceDescriptor">The descriptor for the service</param>
        /// <param name="callHandlerImplementationType">The type of the class that implements the 
        /// service call handler operation type.  If handles no incoming calls, then this can be null.  This 
        /// associated concrete class must have a constructor that takes only IServiceCallContext</param>
        IGenericManagedService AddCreateManagedService(
            IReadOnlyDescriptor serviceDescriptor,
            System.Type callHandlerImplementationType = null
        );

        /// <summary>
        /// Called to add a service to the host.
        /// Note that the service implementation being used must implement
        /// the IMetaService interface for the host to interact with it as well
        /// as a "core" interface that the operation will use. 
        /// </summary>
        /// <param name="service">The service implementing object</param>
        void AddMetaService(
            IMetaService service
        );

        /// <summary>
        /// Adds a built in "lite" version of some platform services for limited use only
        /// </summary>
        /// <param name="service"></param>
        void AddBuiltinService(BuiltinServices serviceType);

        /// <summary>
        /// Final step to finish init activities.  After this, the host will be
        /// "live"
        /// </summary>
        /// <returns></returns>
        void StartHost(
            IEnumerable<string> initialRegistryHostAddresses,
            object startupParameters, 
            bool failIfCannotRegister = false
        );

        /// <summary>
        /// Final step to finish init activities.  After this, the host will be
        /// "live"
        /// </summary>
        /// <returns></returns>
        void StartHost(
            IEnumerable<string> initialRegistryHostAddresses,
            IDictionary<string, object> startupParameters = null, 
            bool failIfCannotRegister = false
        );

        /// <summary>
        /// Final step to finish init activities.  After this, the host will be
        /// "live"
        /// </summary>
        /// <returns></returns>
        Task StartHostAsync(
            IEnumerable<string> initialRegistryHostAddresses,
            object startupParameters, 
            bool failIfCannotRegister = false
        );

        /// <summary>
        /// Final step to finish init activities.  After this, the host will be
        /// "live"
        /// </summary>
        /// <returns></returns>
        Task StartHostAsync(
            IEnumerable<string> initialRegistryHostAddresses,
            IDictionary<string, object> startupParameters = null, 
            bool failIfCannotRegister = false
        );

        void PauseHost();

        void ResumeHost();

        void StopHost();

        Task StopHostAsync();

        void KillHost();

        /// <summary>
        /// Called to pull the latest config information for all services.
        /// </summary>
        /// <returns></returns>
        Task RefreshConfigurationFromSource();

        /// <summary>
        /// Called when the local environment has changed
        /// </summary>
        void SignalLocalEnvironmentChange();

    }
}

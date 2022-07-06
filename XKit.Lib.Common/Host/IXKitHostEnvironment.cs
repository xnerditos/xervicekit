using System;
using System.Collections.Generic;
using XKit.Lib.Common.Config;
using XKit.Lib.Common.Registration;

namespace XKit.Lib.Common.Host {
    
    public interface IXKitHostEnvironment : IXKitEnvironment {
        
        string Address { get; }
        
        bool HasHostedServices { get; }
        
        HealthEnum GetHealth();
        
        IEnumerable<IReadOnlyServiceRegistration> GetHostedServices();
        
        IEnumerable<string> GetCapabilities();
        
        IEnumerable<ServiceInstanceStatus> GetHostedServiceStatuses();
       
        IEnumerable<IManagedService> GetManagedServices(
            string collectionName = null,
            string serviceName = null,
            int? serviceVersion = null
        );

        IManagedService GetManagedService(IReadOnlyDescriptor descriptor);

        IEnumerable<IMetaService> GetMetaServices(
            string serviceName = null
        );

        /// <summary>
        /// Get a startup parameter
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetStartupParameter<T>(string key, T defaultValue = default);

        string DataRootFolderPath { get; }
		
        RunStateEnum HostRunState { get; }
        
        ILocalConfigSessionFactory LocalConfigSessionFactory { get; }

        /// <summary>
        /// The key to identify the host's config document
        /// </summary>
        /// <value></value>
        string ConfigurationDocumentIdentifier { get; }

        /// <summary>
        /// Version of host
        /// </summary>
        int? VersionLevel { get; }

        /// <summary>
        /// Gets an object that has been previously registered for a type.  The type
        /// must be interface.
        /// </summary>
        /// <typeparam name="TRegisteredInterface"></typeparam>
        /// <returns>The corresponding object, either created or cached depending on the registration</returns>
        TRegisteredInterface ObjectRepositoryGetObject<TRegisteredInterface>();

        /// <summary>
        /// Gets an object that has been previously registered for a type.  The type
        /// must be interface.
        /// </summary>
        /// <returns>The corresponding object, either created or cached depending on the registration</returns>
        object ObjectRepositoryGetObject(System.Type interfaceType);

        /// <summary>
        /// Indicates of the type has been registered
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        bool ObjectRepositoryHasObject(System.Type interfaceType);

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Config;
using XKit.Lib.Common.Utility.Extensions;
using XKit.Lib.Common.Services;
using System.Reflection;
using XKit.Lib.Common.Utility;
using XKit.Lib.Host.Config;
using XKit.Lib.Common.Services.Config;
using XKit.Lib.Common.Utility.Collections;
using XKit.Lib.Common.Utility.Threading;
using XKit.Lib.Host.Services;
using XKit.Lib.Host.Helpers;
using System.Threading;
using XKit.Lib.Common.Services.MessageBroker;
using XKit.Lib.Connector.Fabric;
using System.Runtime.CompilerServices;
using XKit.Lib.Common.Utility.ObjectRepository;

namespace XKit.Lib.Host.Management {
    internal class XKitHost : IXKitHost, IXKitHostEnvironment, IXKitEnvironment {
        private readonly int HostVersionLevel;
        #pragma warning disable IDE0052
        private readonly int HostUpdateLevel;
        private readonly int HostPatchLevel;
        #pragma warning restore IDE0052
        private readonly IList<IManagedService> ManagedServices = new SynchronizedList<IManagedService>();
        private readonly IList<IMetaService> MetaServices = new SynchronizedList<IMetaService>();
        private readonly string HostAddress;
        private readonly Func<HealthEnum> HostHealthGetter;
        private readonly IFabricConnector Connector;
        private readonly ILogSessionFactory LogSessionFactory;
        private readonly string LocalMetaDataDbPath;
        private readonly string LocalDataFolderPath;
        private readonly ILocalConfigSessionFactory LocalConfigSessionFactory;
        private IDictionary<string,object> startupParameters;
        private RunStateEnum state = RunStateEnum.Inactive;
        private IConfigReader<HostConfigDocument> hostConfigReader;
        private readonly string configDocumentIdentifier; 
        private readonly Lazy<IConfigClient> configClientLazy;
        private readonly Lazy<IMessageBrokerClient> messagingClientLazy;
        private readonly SemaphoreSlim hostActionSync = new(1, 1);
        private readonly SetOnceOrThrow<ILogSession> log = new();
        private readonly SetOnceOrThrow<IObjectRepository> objectRepository = new();
        private ILogSession Log => log.Value;
        private IConfigClient ConfigClient => configClientLazy.Value;
        private IMessageBrokerClient MessagingClient => messagingClientLazy.Value;

        public XKitHost(
            string hostAddress,
            IFabricConnector hostFabricConnector,
            ILogSessionFactory logSessionFactory,
            ILocalConfigSessionFactory localConfigSessionFactory,
            string localMetaDataDbPath,
            string localDataFolderPath,
            Func<HealthEnum> hostHealthGetter,
            IConfigClient configClient,
            IMessageBrokerClient messagingClient
        ) { 
            this.HostAddress = hostAddress;
            this.Connector = hostFabricConnector;
            this.LogSessionFactory = logSessionFactory;
            this.LocalConfigSessionFactory = localConfigSessionFactory;
            this.LocalMetaDataDbPath = localMetaDataDbPath;
            this.LocalDataFolderPath = localDataFolderPath;
            this.HostHealthGetter = hostHealthGetter;
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            this.HostVersionLevel = version.Major;
            this.HostUpdateLevel = version.Minor;
            this.HostPatchLevel = version.Build;
            this.configDocumentIdentifier = Identifiers.GetHostVersionLevelKey(version.Major);
            objectRepository.Value = ObjectRepositoryFactory.Create();

            this.configClientLazy = new(() => {
                return configClient ?? 
                    new InternalConfigClient(Log, Connector);
            });
            this.messagingClientLazy = new(() => { 
                return messagingClient ??
                    new InternalMessageBrokerClient(Log, Connector);
            }); 
        }


        // =====================================================================
        // IXKitHost
        // =====================================================================

        void IXKitHost.KillHost() {
            BeginHostAction();
            Log.Warning("Killing host!");
            System.Diagnostics.Process.GetCurrentProcess().Kill(); // kerblam!  Ouch!
            EndHostAction();    // We'll never get here, but let's keep it correct.
        }

        void IXKitHost.ResumeHost() {
            hostActionSync.Wait();
            try {
                BeginHostAction();
                if (this.state != RunStateEnum.Paused) {
                    Log?.Warning("Attempt to resume a non-paused host");
                    EndHostAction();
                    return;
                } 
                Log?.Info("Host being resumed");
                foreach(var svc in this.ManagedServices) {
                    svc.ResumeService(Log);
                }            
                this.state = RunStateEnum.Active;
                EndHostAction();
            } catch (Exception ex) {
                EndHostAction(ex.Message);
                throw;
            } finally {
                hostActionSync.Release();
            }
        }

        void IXKitHost.PauseHost() {
            hostActionSync.Wait();
            try { 
                BeginHostAction();
                if (this.state != RunStateEnum.Active) {
                    Log?.Warning("Attempt to pause a host that is not active");
                    EndHostAction();
                    return;
                } 
                Log?.Info("Host being paused");
                foreach(var svc in this.ManagedServices) {
                    svc.PauseService(Log);
                }
                this.state = RunStateEnum.Paused;
                EndHostAction();
            } catch (Exception ex) {
                EndHostAction(ex.Message);
                throw;
            } finally {
                hostActionSync.Release();
            }
        }

        async Task IXKitHost.StopHostAsync() {
            await hostActionSync.WaitAsync();
            try { 
                BeginHostAction();
                if (this.state == RunStateEnum.ShuttingDown || this.state == RunStateEnum.Inactive) {
                    Log?.Warning("Attempt to stop a host that is already shutting down or otherwise inactive");
                    EndHostAction();
                    return;
                }
                await StopHost();
                EndHostAction();
            } catch (Exception ex) {
                EndHostAction(ex.Message);
                throw;
            } finally {
                hostActionSync.Release();
            }
        }

        void IXKitHost.StopHost() {
            hostActionSync.Wait();
            try { 
                BeginHostAction();
                if (this.state == RunStateEnum.ShuttingDown || this.state == RunStateEnum.Inactive) {
                    Log?.Warning("Attempt to stop a host that is already shutting down or otherwise inactive");
                    EndHostAction();
                    return;
                }
                TaskUtil.RunAsyncAsSync(() => StopHost());
            } catch (Exception ex) {
                EndHostAction(ex.Message);
                throw;
            } finally {
                hostActionSync.Release();
            }
        }

        IGenericManagedService IXKitHost.AddCreateManagedService(
            IReadOnlyDescriptor serviceDescriptor,
            System.Type operationType
        ) {
            if (operationType != null) {
                if (!typeof(IServiceOperation).IsAssignableFrom(operationType)) {
                    throw new ArgumentException("Type must implement IServiceApiOperation", nameof(operationType));
                }
                if (operationType.IsInterface) {
                    throw new ArgumentException("Type must be an concrete class", nameof(operationType));
                }
            }

            var genericServiceType = typeof(GenericManagedService<>).MakeGenericType(
                new[] { 
                    operationType ?? typeof(NoOpServiceOperation)
                });

            var serviceInstance = (IGenericManagedService)Activator.CreateInstance(
                genericServiceType, 
                serviceDescriptor.Clone(),
                (IXKitEnvironment)this
            );

            ManagedServices.Add(serviceInstance);
            return serviceInstance;
        } 

        IManagedService IXKitHost.AddManagedService(
            IManagedService service
        ) {
            ManagedServices.Add(service);
            return service;
        } 

        IManagedService IXKitHost.AddManagedService(
            Func<IXKitHostEnvironment, IManagedService> factoryMethod
        ) {
            var service = factoryMethod(this);
            ManagedServices.Add(service);
            return service;
        } 

        IMetaService IXKitHost.AddMetaService(
            IMetaService service
        ) {
            MetaServices.Add(service);
            return service;
        }

        IMetaService IXKitHost.AddMetaService(
            Func<IXKitHostEnvironment, IMetaService> factoryMethod
        ) {
            var service = factoryMethod(this);
            MetaServices.Add(service);
            return service;
        }

        void IXKitHost.AddBuiltinService(
            BuiltinServices serviceType
        ) {
            IManagedService service = serviceType switch {
                BuiltinServices.MessageBrokerLocalOnly => new BuiltinMessageBrokerService(this),
                _ => throw new Exception("Build in service not found"),
            };
            ManagedServices.Add(service);
        } 

        async Task IXKitHost.StartHostAsync(
            IEnumerable<string> initialRegistryHostAddresses,
            object startupParameters, 
            bool failIfCannotRegister
        ) {
            await hostActionSync.WaitAsync();
            try { 
                if (this.state != RunStateEnum.Inactive) {
                    return;
                }
                this.state = RunStateEnum.StartingUp;
                await StartHost(
                    initialRegistryHostAddresses,
                    startupParameters?.FieldsToDictionary(), 
                    failIfCannotRegister
                );
            } finally {
                hostActionSync.Release();
            }
        }

        async Task IXKitHost.StartHostAsync(
            IEnumerable<string> initialRegistryHostAddresses,
            IDictionary<string, object> startupParameters, 
            bool failIfCannotRegister
        ) {
            await hostActionSync.WaitAsync();
            try { 
                if (this.state != RunStateEnum.Inactive) {
                    return;
                }
                this.state = RunStateEnum.StartingUp;
                await StartHost(
                    initialRegistryHostAddresses,
                    startupParameters, 
                    failIfCannotRegister
                );
            } finally {
                hostActionSync.Release();
            }
        }

        void IXKitHost.StartHost(
            IEnumerable<string> initialRegistryHostAddresses,
            object startupParameters, 
            bool failIfCannotRegister
        ) {
            hostActionSync.Wait();
            try { 
                if (this.state != RunStateEnum.Inactive) {
                    return;
                }
                this.state = RunStateEnum.StartingUp;
                TaskUtil.RunAsyncAsSync(async () => {
                    await StartHost(
                        initialRegistryHostAddresses,
                        startupParameters?.FieldsToDictionary(), 
                        failIfCannotRegister
                    );
                });
            } finally {
                hostActionSync.Release();
            }
        }

        void IXKitHost.StartHost(
            IEnumerable<string> initialRegistryHostAddresses,
            IDictionary<string, object> startupParameters, 
            bool failIfCannotRegister
        ) {
            hostActionSync.Wait();
            try { 
                if (this.state != RunStateEnum.Inactive) {
                    return;
                }
                this.state = RunStateEnum.StartingUp;
                TaskUtil.RunAsyncAsSync(async () => {
                    await StartHost(
                        initialRegistryHostAddresses,
                        startupParameters, 
                        failIfCannotRegister
                    );
                });
            } finally {
                hostActionSync.Release();
            }
        }

        async Task IXKitHost.RefreshConfigurationFromSource() {
            hostActionSync.Wait();
            try { 
                BeginHostAction();
                if (this.state == RunStateEnum.Inactive || this.state == RunStateEnum.ShuttingDown) {
                    Log?.Warning("Attempt to refresh configuration for a host that is shutting down or otherwise inactive");
                    EndHostAction();
                    return;
                }

                var updatedConfigIdentifiers = await CallConfigurationAndRefreshLocalConfig(
                    fatalIfUnavailable: false
                );
                if (updatedConfigIdentifiers.Contains(this.configDocumentIdentifier)) {
                    ActOnConfigurationChange();
                }

                foreach(var svc in this.MetaServices) {
                    if (updatedConfigIdentifiers.Contains(svc.ConfigurationDocumentIdentifier)) {
                        svc.SignalEnvironmentChange(Log);
                    }
                }
                foreach(var svc in this.ManagedServices) {
                    if (updatedConfigIdentifiers.Contains(svc.ConfigurationDocumentIdentifier)) {
                        svc.SignalEnvironmentChange(Log);
                    }
                }
                EndHostAction();
            } catch (Exception ex) {
                EndHostAction(ex.Message);
                throw;
            } finally {
                hostActionSync.Release();
            }
        }
 
        string IXKitHost.LocalDataFolderPath => LocalDataFolderPath;

        string IXKitHost.MetaDataDbPath => LocalMetaDataDbPath;

        void IXKitHost.SignalConfigurationChange()
            => ActOnConfigurationChange();
            
        void IXKitHost.ObjectRepositoryRegisterSingleton<TConcreteType>(
            TConcreteType obj, 
            params Type[] forTypes
        ) => objectRepository.Value.RegisterObjectSingleton(obj, forTypes);

        void IXKitHost.ObjectRepositoryRegisterFactory<TConcreteType>(
            Func<TConcreteType> createMethod, 
            params Type[] forTypes
        ) => objectRepository.Value.RegisterObjectFactory(createMethod, forTypes);

        void IXKitHost.ClearObjectRepository() => objectRepository.Value.Clear();

        // =====================================================================
        // IXKitEnvironment
        // =====================================================================

        string IXKitEnvironment.FabricId => this.Connector.FabricId;

        IEnumerable<IReadOnlyDescriptor> IXKitEnvironment.GetDependencies()
            => GetDependencies();

        ILogSessionFactory IXKitEnvironment.LogSessionFactory => LogSessionFactory;

        IFabricConnector IXKitEnvironment.Connector => Connector;
            
        // =====================================================================
        // IXKitHostEnvironment
        // =====================================================================

        TRegisteredInterface IXKitHostEnvironment.ObjectRepositoryGetObject<TRegisteredInterface>() 
            => objectRepository.Value.GetObject<TRegisteredInterface>();

        object IXKitHostEnvironment.ObjectRepositoryGetObject(Type interfaceType) 
            => objectRepository.Value.GetObject(interfaceType);

        bool IXKitHostEnvironment.ObjectRepositoryHasObject(Type interfaceType) 
            => objectRepository.Value.HasObject(interfaceType);

        bool IXKitHostEnvironment.HasHostedServices 
            => ManagedServices.Any();            

        ILocalConfigSessionFactory IXKitHostEnvironment.LocalConfigSessionFactory 
            => LocalConfigSessionFactory;

        int? IXKitHostEnvironment.VersionLevel => HostVersionLevel;

        string IXKitHostEnvironment.ConfigurationDocumentIdentifier => this.configDocumentIdentifier;
        
        T IXKitHostEnvironment.GetStartupParameter<T>(string key, T defaultValue) 
            => GetStartupParameter<T>(key, defaultValue);

        string IXKitHostEnvironment.Address => this.HostAddress;

        string IXKitHostEnvironment.DataRootFolderPath => this.LocalDataFolderPath;

        RunStateEnum IXKitHostEnvironment.HostRunState => this.state;

        HealthEnum IXKitHostEnvironment.GetHealth() 
            => GetHealth();

        IEnumerable<ServiceInstanceStatus> IXKitHostEnvironment.GetHostedServiceStatuses() 
            => GetLocalInstanceStatuses();

        IEnumerable<IReadOnlyServiceRegistration> IXKitHostEnvironment.GetHostedServices() 
            => GetLocalServiceRegistrations();

        IEnumerable<string> IXKitHostEnvironment.GetCapabilities() 
            => MetaServices.Select(svc => svc.CapabilityKeyName);

        IEnumerable<IManagedService> IXKitHostEnvironment.GetManagedServices(
            string collectionName,
            string serviceName,
            int? serviceVersion
        ) => ManagedServices
            .Where(
                s => (collectionName == null || s.Descriptor.Collection == collectionName) &&
                     (serviceName == null || s.Descriptor.Name == serviceName) &&
                     (serviceVersion == null || s.Descriptor.Version == serviceVersion) 
            ).ToArray();

        IManagedService IXKitHostEnvironment.GetManagedService(IReadOnlyDescriptor descriptor)
            => ManagedServices.Where(s => s.Descriptor.IsSameService(descriptor)).FirstOrDefault();
            
        IEnumerable<IMetaService> IXKitHostEnvironment.GetMetaServices(
            string serviceName
        ) => MetaServices
            .Where(
                s => s.Descriptor.Name == serviceName 
            );
        
        // =====================================================================
        // Internal for testing support
        // =====================================================================

        // =====================================================================
        // private
        // =====================================================================

        private T GetStartupParameter<T>(string key, T defaultValue = default) {
            if (!startupParameters.TryGetValue(key, out object val)) {
                return defaultValue;
            }
            return (T)val;
        }

        private async Task StartHost(
            IEnumerable<string> initialRegistryHostAddresses,
            IDictionary<string, object> startupParameters, 
            bool failIfCannotRegister
        ) {
            log.Value = LogSessionFactory.CreateLogSession(
                originatorName: Identifiers.NameOriginatorAsHost,
                originatorVersion: HostVersionLevel,
                originatorFabricId: Connector.FabricId
            );
            BeginHostAction();

            try {

                this.startupParameters = startupParameters ?? new Dictionary<string, object>();
                var platformServices = this.ManagedServices.Where(isPlatformService).ToArray();
                var nonPlatformServices = this.ManagedServices.Where(svc => !isPlatformService(svc)).ToArray();
                var correlationId = Identifiers.GenerateIdentifier();
                
                foreach(var svc in this.MetaServices) {
                    svc.StartService(Log);
                }

                foreach(var svc in platformServices) {
                    svc.StartService(Log);
                }

                await RegisterHostWithFabric(
                    initialRegistryHostAddresses,
                    failIfCannotRegister
                );
                                
                if (GetStartupParameter<bool>(HostStartupParameterKeys.REFRESH_CONFIG_ON_STARTUP, true)) {
                    var fatalIfUnavailable = GetStartupParameter<bool>(HostStartupParameterKeys.FAIL_IF_CONFIG_UNAVAILABLE_ON_STARTUP, false);
                    var updatedConfigIdentifiers = await CallConfigurationAndRefreshLocalConfig(
                        fatalIfUnavailable: fatalIfUnavailable
                    );

                    // signal env change because the meta services and the platform services were already started.

                    foreach(var svc in this.MetaServices) {
                        if (updatedConfigIdentifiers.Contains(svc.ConfigurationDocumentIdentifier)) {
                            svc.SignalEnvironmentChange(Log);
                        }
                    }
                    foreach(var svc in platformServices) {
                        if (updatedConfigIdentifiers.Contains(svc.ConfigurationDocumentIdentifier)) {
                            svc.SignalEnvironmentChange(Log);
                        }
                    }
                }

                this.hostConfigReader = ConfigReaderFactory.CreateForHost<HostConfigDocument>(this.HostVersionLevel, LocalConfigSessionFactory);

                foreach(var svc in nonPlatformServices) {
                    svc.StartService(Log);
                }

                this.state = RunStateEnum.Active;

                foreach(var svc in platformServices) {
                    svc.SignalHostStartupComplete(Log);
                }
                foreach(var svc in nonPlatformServices) {
                    svc.SignalHostStartupComplete(Log);
                }

                if (GetStartupParameter<bool>(HostStartupParameterKeys.REGISTER_SUBSCRIPTIONS_ON_STARTUP, true)) {
                    var fatalIfUnavailable = GetStartupParameter<bool>(HostStartupParameterKeys.FAIL_IF_MESSAGE_BROKER_UNAVAILABLE_ON_STARTUP, true);

                    List<Subscription> subscriptionsToSubmit = new List<Subscription>();
                    foreach(var svc in this.MetaServices) {
                        subscriptionsToSubmit.AddRange(
                            svc.CommandSubscriptions.Select(s => new Subscription {
                                ErrorHandling = s.ErrorHandling,
                                FailureDelaysToRetryMs = s.FailureDelaysToRetryMs?.ToArray(),
                                MaxConsecutiveFailures = s.MaxConsecutiveFailures,
                                MaxDeliveryRetries = s.MaxDeliveryRetries,
                                MessageTypeName = s.MessageTypeName,
                                Policy = s.Policy?.Clone(),
                                Recipient = svc.Descriptor.Clone(),
                                RecipientHostId = s.RecipientHostId
                            })
                        );
                        subscriptionsToSubmit.AddRange(
                            svc.EventSubscriptions.Select(s => new Subscription {
                                ErrorHandling = s.ErrorHandling,
                                FailureDelaysToRetryMs = s.FailureDelaysToRetryMs?.ToArray(),
                                MaxConsecutiveFailures = s.MaxConsecutiveFailures,
                                MaxDeliveryRetries = s.MaxDeliveryRetries,
                                MessageTypeName = s.MessageTypeName,
                                Policy = s.Policy?.Clone(),
                                Recipient = svc.Descriptor.Clone(),
                                RecipientHostId = s.RecipientHostId
                            })
                        );
                    }

                    foreach(var svc in this.ManagedServices) {
                        subscriptionsToSubmit.AddRange(
                            svc.CommandSubscriptions.Select(s => new Subscription {
                                ErrorHandling = s.ErrorHandling,
                                FailureDelaysToRetryMs = s.FailureDelaysToRetryMs?.ToArray(),
                                MaxConsecutiveFailures = s.MaxConsecutiveFailures,
                                MaxDeliveryRetries = s.MaxDeliveryRetries,
                                MessageTypeName = s.MessageTypeName,
                                Policy = s.Policy?.Clone(),
                                Recipient = svc.Descriptor.Clone(),
                                RecipientHostId = s.RecipientHostId
                            })
                        );
                        subscriptionsToSubmit.AddRange(
                            svc.EventSubscriptions.Select(s => new Subscription {
                                ErrorHandling = s.ErrorHandling,
                                FailureDelaysToRetryMs = s.FailureDelaysToRetryMs?.ToArray(),
                                MaxConsecutiveFailures = s.MaxConsecutiveFailures,
                                MaxDeliveryRetries = s.MaxDeliveryRetries,
                                MessageTypeName = s.MessageTypeName,
                                Policy = s.Policy?.Clone(),
                                Recipient = svc.Descriptor.Clone(),
                                RecipientHostId = s.RecipientHostId
                            })
                        );
                    }

                    if (subscriptionsToSubmit.Count > 0) {
                        await CallMessageBrokerAndRegisterSubscriptions(
                            subscriptionsToSubmit,
                            fatalIfUnavailable: fatalIfUnavailable
                        );
                    }
                }
                EndHostAction();
            } catch(Exception ex) {
                EndHostAction(ex.Message);
                throw;
            }

            // ------------------------------------------------------------------------
            static bool isPlatformService(IManagedService svc) 
                => svc.Descriptor.Collection == StandardConstants.Managed.Collections.Platform;
        }

        private async Task StopHost() {
            BeginHostAction();
            try {
                this.state = RunStateEnum.ShuttingDown;
                Log?.Info("Host being stopped");

                var platformServices = this.ManagedServices.Where(isPlatformService).ToArray();
                var nonPlatformServices = this.ManagedServices.Where(svc => !isPlatformService(svc)).ToArray();
                
                foreach(var svc in nonPlatformServices) {
                    svc.StopService(Log);
                }

                foreach(var svc in platformServices) {
                    svc.StopService(Log);
                }

                foreach(var svc in this.MetaServices) {
                    svc.StopService(Log);
                }

                bool unRegisterSuccessful = await Connector.Unregister(Log);

                if (!unRegisterSuccessful) {
                    Log?.Warning("Unregistering host failed");
                }

                foreach(var svc in nonPlatformServices) {
                    svc.SignalHostShutdownComplete(Log);
                }
                foreach(var svc in platformServices) {
                    svc.SignalHostShutdownComplete(Log);
                }

                this.state = RunStateEnum.Inactive;
                EndHostAction();
            } catch(Exception ex) {
                EndHostAction(ex.Message);
                throw;
            }

            // ------------------------------------------------------------------------
            static bool isPlatformService(IManagedService svc) 
                => svc.Descriptor.Collection == StandardConstants.Managed.Collections.Platform;
        }

        private async Task RegisterHostWithFabric(
            IEnumerable<string> initialRegistryHostAddresses,
            bool failIfCannotRegister
        ) {
            bool registrationSuccessful = await Connector.Register(
                Log,
                initialRegistryHostAddresses,
                this
            );
            if (!registrationSuccessful && failIfCannotRegister) {
                throw new Exception("Host registration failed");
            }
        }

        private async Task CallMessageBrokerAndRegisterSubscriptions(
            IReadOnlyList<Subscription> subscriptions,
            bool fatalIfUnavailable = false
        ) {
            var result = (await MessagingClient.ExecuteWith<SubscribeRequest>(
                x => x.Subscribe(null),
                new SubscribeRequest {
                    Subscriptions = subscriptions.ToArray()
                },
                log: Log
            ))[0];

            if (!result.Completed) {
                const string message = "Message Broker service is not available";
                Log?.Warning(message);
                if (fatalIfUnavailable) {
                    throw new Exception(message);
                }
                return;
            }

            if (result.HasError) {
                const string message = "Subscribe call failed";
                Log?.Warning(message);
                if (fatalIfUnavailable) {
                    throw new Exception(message);
                }
                return;
            }
        }

        private async Task<HashSet<string>> CallConfigurationAndRefreshLocalConfig(
            bool fatalIfUnavailable = false
        ) {
            HashSet<string> updatedConfigIdentifiers = new HashSet<string>();
            
            var result = (await ConfigClient.ExecuteWith<ConfigServiceQueryRequest, ConfigServiceQueryResponse>(
                x => x.QueryConfig(null),
                new ConfigServiceQueryRequest {
                    HostVersionLevel = this.HostVersionLevel,
                    ServiceKeys = 
                        ManagedServices
                        .Select(svc => svc.ConfigurationDocumentIdentifier)
                        .Union(MetaServices.Select(svc => svc.ConfigurationDocumentIdentifier))
                        .ToArray()
                },
                log: Log
            ))[0];

            if (!result.Completed) {
                const string message = "Configuration service is not available";
                Log?.Warning(message);
                if (fatalIfUnavailable) {
                    throw new Exception(message);
                }
                return updatedConfigIdentifiers;
            }

            if (result.HasError) {
                const string message = "Configuration service call failed";
                Log?.Warning(message);
                if (fatalIfUnavailable) {
                    throw new Exception(message);
                }
                return updatedConfigIdentifiers;
            }

            var response = result.ResponseBody;
            await updateLocalConfig(this.configDocumentIdentifier, response.HostConfig.ToJson());

            if (response?.ServiceConfigJson?.Count > 0) {
                foreach(var key in response.ServiceConfigJson.Keys) {
                    var json = response.ServiceConfigJson[key];
                    await updateLocalConfig(key, json);
                }
            }

            return updatedConfigIdentifiers;

            // -----------------------------------------------------------------

            async Task updateLocalConfig(string documentIdentifier, string configJson) {
                if (!string.IsNullOrEmpty(configJson)) {
                    updatedConfigIdentifiers.Add(documentIdentifier);
                    var localConfigSession = LocalConfigSessionFactory.Create(documentIdentifier); 
                    await localConfigSession.UpdateConfigJson(configJson);
                }
            }
        }

        private IEnumerable<Descriptor> GetDependencies() {
            var dependencies = new Dictionary<string,IReadOnlyDescriptor>();

            foreach(var svc in MetaServices) {
                foreach(var dep in svc.Dependencies) {
                    string key = Identifiers.GetServiceFullRegistrationKey(dep);
                    dependencies[key] = dep;
                }
            }

            foreach(var svc in ManagedServices) {
                foreach(var dep in svc.Dependencies) {                    
                    string key = Identifiers.GetServiceFullRegistrationKey(dep);
                    dependencies[key] = dep;
                }
            }

            return dependencies.Values.Select(d => d.Clone());
        }

        private IEnumerable<ServiceRegistration> GetLocalServiceRegistrations() 
            =>  (from svc in ManagedServices
                let descriptor = svc.Descriptor.Clone()
                select new ServiceRegistration {
                    RegistrationKey = Common.Utility.Identifiers.GetServiceFullRegistrationKey(svc.Descriptor),
                    Descriptor = descriptor,
                    CallPolicy = svc.CallPolicy?.Clone(),
                    Instances = new List<ServiceInstance> { 
                        new ServiceInstance {
                            HostFabricId = this.Connector.FabricId,
                            InstanceId = svc.InstanceId,
                            RegistrationKey = Common.Utility.Identifiers.GetServiceFullRegistrationKey(svc.Descriptor),
                            Status = svc.GetServiceStatus(),
                            HostAddress = HostAddress,
                            Descriptor = descriptor
                        }
                    }
                }).Union(
                from svc in MetaServices
                let descriptor = svc.Descriptor.Clone()
                select new ServiceRegistration {
                    RegistrationKey = Common.Utility.Identifiers.GetServiceFullRegistrationKey(svc.Descriptor),
                    Descriptor = descriptor,
                    CallPolicy = svc.CallPolicy?.Clone(),
                    Instances = new List<ServiceInstance> { 
                        new ServiceInstance {
                            HostFabricId = this.Connector.FabricId,
                            InstanceId = svc.InstanceId,
                            RegistrationKey = Common.Utility.Identifiers.GetServiceFullRegistrationKey(svc.Descriptor),
                            Status = svc.GetServiceStatus(),
                            HostAddress = HostAddress,
                            Descriptor = descriptor
                        }
                    }
                });

        private IEnumerable<ServiceInstanceStatus> GetLocalInstanceStatuses() 
            =>  (from svc in ManagedServices
                select svc.GetServiceStatus())
                .Union(
                from svc in MetaServices
                select svc.GetServiceStatus());
                
         private HealthEnum GetHealth()
            => HostHealthGetter != null ? HostHealthGetter() : HealthEnum.Unknown;

        private void ActOnConfigurationChange()
            => this.hostConfigReader.InvalidateCache();
        
        private void BeginHostAction(
            [CallerMemberName] string callerMemberName = ""         
        ) {
            Log?.Begin(
                LogContextTypeEnum.HostAction,
                callerMemberName
            );
        }

        private void EndHostAction(
            string message = null
        ) {
            Log?.End(
                message == null ? LogResultStatusEnum.Success : LogResultStatusEnum.NonRetriableError,
                message
            );
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Utility;
using XKit.Lib.Common.Utility.Extensions;
using XKit.Lib.Connector.Dependency;
using XKit.Lib.Common.Client;
using ServiceConstants = XKit.Lib.Common.Services.StandardConstants;
using System.Collections.Concurrent;
using XKit.Lib.Common.Services.Registry;

namespace XKit.Lib.Connector.Fabric {
    
    internal class FabricConnector : IFabricConnector {
        public enum MonitorCodes { 

            NoInstanceAvailable,
            ServiceNotAvailableFailure,
            ServiceNotAvailable,
            NewDependencyRequested,
            RefreshingWithRegistry,
            RegistrationRefreshFailed,
            InitialRegistrationFailed,
            NoRegistryServiceFound,
            ResolvingForServiceDependencyFailed,
            ResolvingForRegistryServiceFailed,
            RegistrationRefreshRegistryCallRouterNotAvailable
        }

        private const int ReAttemptDependencyResolutionTimeoutSeconds = 120;
        private readonly ConcurrentDictionary<string, IReadOnlyServiceRegistration> dependencyRegistrations = new();
        private readonly IServiceCallRouterFactory routerFactory;
        private readonly SetOnceOrThrow<IRegistryClient> externalRegistryClient = new();
        private IRegistryClient ExternalRegistryClient => externalRegistryClient.Value;

        private ConcurrentDictionary<string, IServiceCallRouter> callRouters = new();
        private IReadOnlyList<IReadOnlyDescriptor> dependencies;
        private DateTime? cacheExpiration;
        private readonly SetOnceOrThrow<string> fabricId = new();
        private readonly IReadOnlyList<IInstanceClientFactory> instanceClientFactories;
        private readonly SetOnceOrThrow<IXKitEnvironment> xkitEnvironment = new();
        private readonly SetOnceOrThrow<IXKitHostEnvironment> hostEnvironment = new();
        private IXKitEnvironment XKitEnvironment => xkitEnvironment.Value;
        private IXKitHostEnvironment HostEnvironment => hostEnvironment.Value;
        private string FabricId => fabricId.Value;
        private bool isRegisteredWithFabric = false;
        bool IsHost => this.HostEnvironment != null;

        public FabricConnector(
            IServiceCallRouterFactory routerFactory,
            IList<IInstanceClientFactory> instanceClientFactories,
            IRegistryClient registryClient = null
        ) {
            this.routerFactory = routerFactory ??
                throw new ArgumentNullException(nameof(routerFactory));
            instanceClientFactories = instanceClientFactories ?? 
                throw new ArgumentNullException(nameof(instanceClientFactories));
            if (!instanceClientFactories.Any()) {
                throw new ArgumentException("list cannot be empty", nameof(instanceClientFactories));
            }
            this.instanceClientFactories = instanceClientFactories.ToList();
            externalRegistryClient.Value = registryClient;
        }

        // =====================================================================
        // Internal Testing
        // =====================================================================
        internal IEnumerable<IReadOnlyServiceRegistration> GetDependencyRegistrations() 
            => dependencyRegistrations.Values.ToArray();

        // =============================================================================
        // IFabricConnector
        // =============================================================================

        string IFabricConnector.Initialize() {
            if (fabricId.HasValue) {
                throw new Exception("Host connector already initialized");
            }
            return Initialize();
        }

        Task<bool> IFabricConnector.Register(
            ILogSession log,
            IEnumerable<string> initialRegistryHostAddresses,
            IXKitEnvironment xkitEnvironment,
            bool fatalIfUnableToRegister
        ) => Register(
            log,
            initialRegistryHostAddresses,
            xkitEnvironment,
            xkitEnvironment as IXKitHostEnvironment,
            fatalIfUnableToRegister
        );

        Task<bool> IFabricConnector.Register(
            ILogSession log,
            IEnumerable<string> initialRegistryHostAddresses,
            IXKitHostEnvironment hostEnvironment,
            bool fatalIfUnableToRegister
        ) => Register(
            log,
            initialRegistryHostAddresses,
            hostEnvironment,
            hostEnvironment,
            fatalIfUnableToRegister
        );

        async Task<bool> IFabricConnector.Refresh(
            ILogSession log
        ) {
            if (!isRegisteredWithFabric) {
                return await RegisterInFabric(
                    log: log,
                    fatalIfUnableToRegister: false
                );
            } 
            return await RefreshFabricRegistration(log);
        }        

        Task IFabricConnector.ForceResetTopologyMap(
            IReadOnlyServiceTopologyMap map
        ) {
            ResetTopologyMapForDependencies(map);
            return Task.CompletedTask;
        } 

        async Task<bool> IFabricConnector.Unregister(ILogSession log) {
            return await Unregister(log);
        }

        string IFabricConnector.FabricId => FabricId;
        
        bool IFabricConnector.IsHost => IsHost;

        async Task<IServiceCallRouter> IFabricConnector.CreateCallRouter(
            IReadOnlyDescriptor target, 
            ILogSession log,
            bool failIfNotAvailable,
            bool allowRegistryRefreshIfRequested
        ) {
            target = target ?? throw new ArgumentNullException(nameof(target));

            var router = await CreateOrGetServiceCallRouterEx(
                target: target, 
                log: log,
                addNewTargetsToDependencyListAndRefresh: allowRegistryRefreshIfRequested
            );

            if (router == null) {
                if (failIfNotAvailable) {
                    log?.Error(
                        "Service not available", 
                        new { target }, 
                        code: MonitorCodes.ServiceNotAvailableFailure
                    );
                    throw new Exception($"Service not available: {Identifiers.GetServiceFullRegistrationKey(target)}");
                } else {
                    log?.Warning(
                        "Service not available", 
                        new { target }, 
                        code: MonitorCodes.ServiceNotAvailable
                    );
                }
                return null;
            }

            return router;
        }

        // =============================================================================
        // Workers
        // =============================================================================

        async Task<bool> Register(
            ILogSession log,
            IEnumerable<string> initialRegistryHostAddresses,
            IXKitEnvironment xkitEnvironment,
            IXKitHostEnvironment hostEnvironment,
            bool fatalIfUnableToRegister
        ) {
            xkitEnvironment = xkitEnvironment ?? throw new ArgumentNullException(nameof(xkitEnvironment));
            if (xkitEnvironment.FabricId != FabricId) {
                throw new ArgumentException("Local fabric environment does not match this fabric connector id");
            }
            initialRegistryHostAddresses ??= Array.Empty<string>();

            this.xkitEnvironment.Value = xkitEnvironment;
            this.hostEnvironment.Value = hostEnvironment;
            InitializeAccess(initialRegistryHostAddresses);

            return await RegisterInFabric(
                log, 
                fatalIfUnableToRegister
            );
        }

        private async Task<bool> Unregister(ILogSession log) {

            IRegistryClient registryClient = ExternalRegistryClient;
            if (registryClient == null) {
                registryClient = AttemptCreateRegistryClient(log);
            } 
            if (registryClient == null) {
                return false;
            }

            var response = await registryClient.Unregister(new UnregisterRequest { FabricId = FabricId });
            return !response.HasError;
        }

        private IRegistryClient AttemptCreateRegistryClient(ILogSession log) {

            IServiceCallRouter callRouter = TryGetCachedServiceCallRouter(ServiceConstants.Managed.StandardServices.Registry.Descriptor);
            if (callRouter == null) {
                IReadOnlyServiceRegistration registryServiceRegistration = TryGetDependencyServiceRegistration(ServiceConstants.Managed.StandardServices.Registry.Descriptor);

                if (registryServiceRegistration == null) {
                    log?.WarningAs(
                        $"Could not obtain dependency for Registry service", 
                        code: MonitorCodes.ResolvingForRegistryServiceFailed
                    );
                    return null;
                }

                var registryInstanceClients = CreateInstanceClients(registryServiceRegistration);
                if (!registryInstanceClients.Any()) {
                    log?.Erratum(
                        "Could not find create any instance clients for Registry service",
                        attributes: new { serviceTarget = registryServiceRegistration }.FieldsToDictionary() 
                    );
                    return null;
                }
                
                callRouter = routerFactory.Create(
                    registryServiceRegistration,
                    DateTime.MaxValue,
                    registryInstanceClients
                );

                AddCachedCallRouter(
                    ServiceConstants.Managed.StandardServices.Registry.Descriptor, 
                    callRouter
                );
            }

            return new InternalRegistryClient(
                log,
                this
            );                
        }

        private string Initialize() {
            fabricId.Value = Identifiers.GenerateIdentifier();
            return fabricId;
        }

        private void InitializeAccess(
            IEnumerable<string> initialRegistryHostAddresses
        ) {
            foreach(var instanceClientFactory in this.instanceClientFactories) {
                instanceClientFactory.InitializeFactory(XKitEnvironment);
            }

            // set up initial access to the registry service. 
            // This involves essentially priming the cached dependencies with a mock entry that 
            // points to the initial addresses we have for the registry service.

            if (initialRegistryHostAddresses.Any()) {
                lock(dependencyRegistrations) {
                    var registryServiceDescriptor = ServiceConstants.Managed.StandardServices.Registry.Descriptor.Clone();
                    
                    
                    AddCachedDependencyServiceRegistration(new ServiceRegistration {
                        Descriptor = registryServiceDescriptor,
                        RegistrationKey = Identifiers.GetServiceFullRegistrationKey(registryServiceDescriptor),
                        Instances = initialRegistryHostAddresses.Select(
                            registryHostAddress => new ServiceInstance {
                                HostAddress = isSameHost(registryHostAddress) ? HostEnvironment?.Address : registryHostAddress,
                                HostFabricId = isSameHost(registryHostAddress) ? this.FabricId : null,
                                Descriptor = registryServiceDescriptor,
                                Status = new ServiceInstanceStatus {
                                    Availability = AvailabilityEnum.Serving5,
                                    Health = HealthEnum.Unknown,
                                    RunState = RunStateEnum.Active
                            }
                        }).ToList()
                    });
                }
            }

            bool isSameHost(string addr) {
                // this is 1) a host and 2) the address matches either the local address flag or else 
                // there is an address and it matches the registry address given
                return IsHost && (
                    addr.Equals(HostConstants.LocalHostAddressFlag) ||
                    addr.Equals(HostEnvironment?.Address ?? "----", StringComparison.CurrentCultureIgnoreCase)
                );
            }
        }

        private async Task<bool> RegisterInFabric(
            ILogSession log,
            bool fatalIfUnableToRegister
        ) {
            
            dependencies = XKitEnvironment
                .GetDependencies()
                .Select(d => (IReadOnlyDescriptor) d.Clone())
                .ToArray();

            var registration = new FabricRegistration {
                FabricId = XKitEnvironment.FabricId,
                Capabilities = HostEnvironment?.GetCapabilities()?.ToList(),
                Address = HostEnvironment?.Address,
                Dependencies = dependencies?.Select(d => d.Clone()).ToList(),
                HostedServices = HostEnvironment?.GetHostedServices().Select(s => s.Clone()).ToList(),
                Status = new FabricStatus {
                    FabricId = XKitEnvironment.FabricId,
                    Health = HostEnvironment?.GetHealth(),
                    RunState = HostEnvironment?.HostRunState
                }
            };

            ServiceCallResult<ServiceTopologyMap> registrationResult;
            IRegistryClient registryClient = ExternalRegistryClient;
            if (registryClient == null) {
                registryClient = AttemptCreateRegistryClient(log);
            } 

            if (registryClient != null) {
                
                log?.Trace("Have access to the registryServiceRegistration, using it to register in the fabric");
                registrationResult = await registryClient.Register(registration);

                if (registrationResult.Completed && registrationResult.ImmediateSuccess) {
                    isRegisteredWithFabric = true;
                    ResetTopologyMapForDependencies(registrationResult.ResponseBody);
                    return true;
                } 
                
                log?.Warning("Unable to register with the service fabric.  Temporarily falling back to local services only.", code: MonitorCodes.InitialRegistrationFailed);
                
            } else {
                log?.Warning("Could not get the registryServiceRegistration client.  Falling back on local services only.", code: MonitorCodes.NoRegistryServiceFound);
            }

            // if we get here, we were unable to register

            if (fatalIfUnableToRegister) {
                throw new Exception("Unable to register");
            }

            var haveRegistryClient = ExternalRegistryClient != null;

            // fall back on getting as much as we can from the local host environment
            ResetTopologyMapForDependencies(
                CreateLocalTopologyMap(
                    registration,
                    temporary: haveRegistryClient   // If we have a client, then we want to try again (temporary failure)
                ));

            return haveRegistryClient;              // If we have a client, then we can consider this a success for now.
        }

        // =============================================================================
        // Private utility
        // =============================================================================

        // -----------------------------------------------------------------------------
        // Stuff for instantiating ServiceCallRouter's

        private async Task<IServiceCallRouter> CreateOrGetServiceCallRouterEx(
            IReadOnlyDescriptor target, 
            ILogSession log,
            bool addNewTargetsToDependencyListAndRefresh
        ) {

            IServiceCallRouter callRouter = null;

            var now = DateTime.UtcNow;
            var cacheExpired = now > this.cacheExpiration.GetValueOrDefault(DateTime.MaxValue);

            if (!cacheExpired) {
                callRouter = TryGetCachedServiceCallRouter(target);
                if (callRouter != null) {
                    return callRouter;
                }
            }

            // 
            // create the call router and cache it
            //

            IReadOnlyServiceRegistration registration;

            registration = TryGetDependencyServiceRegistration(target);
            List<IReadOnlyDescriptor> newDependencies = null;
            bool forceRefresh = false;
            lock(dependencies) {
                if (registration == null && !dependencies.Any(svc => svc.IsSameService(target))) {

                    log?.Trace(
                        message: "Target is new dependency",
                        code: MonitorCodes.NewDependencyRequested,
                        attributes: new { target }
                    );

                    if (addNewTargetsToDependencyListAndRefresh) {
                        newDependencies = new List<IReadOnlyDescriptor> {
                            target
                        };
                        // the new dependency will be added when the Registry call returns
                        //dependencies.Add(target.Clone()); 
                        forceRefresh = true;
                    }
                } 
            }

            if (cacheExpired || forceRefresh) {
                // if the cache is expired or it's a new dependency, do a full refresh of our
                // host registration (which will get any new dependencies added)

                log?.Trace(
                    message: "Forcing refresh with Registry service",
                    code: MonitorCodes.RefreshingWithRegistry,
                    attributes: new { cacheExpired, forceRefresh }
                );

                await RefreshFabricRegistration(
                    log,
                    newDependencies
                );
                registration = TryGetDependencyServiceRegistration(target);
            } 

            if (registration == null) {
                log?.Warning(
                    $"Could not obtain service registration for {Identifiers.GetServiceFullRegistrationKey(target)}", 
                    code: MonitorCodes.ResolvingForServiceDependencyFailed
                );
                return null;
            }

            var instanceClients = CreateInstanceClients(registration);
            if (!instanceClients.Any()) {
                // no instance clients could reach this target
                return null;
            }

            callRouter = routerFactory.Create(
                registration,
                cacheExpiration.GetValueOrDefault(DateTime.MaxValue),
                instanceClients
            );

            AddCachedCallRouter(target, callRouter);
            return callRouter;
        }

        private IEnumerable<IInstanceClient> CreateInstanceClients(
            IReadOnlyServiceRegistration serviceRegistration
        ) {
            
            var instanceClients = new List<IInstanceClient>();
            
            foreach (var instance in serviceRegistration.Instances) {
                IInstanceClient instanceClient = TryCreateInstanceClientForService(instance);
                if (instanceClient != null) {
                    instanceClients.Add(instanceClient);
                }
            }

            return instanceClients;
        }

        private IInstanceClient TryCreateInstanceClientForService(
            IReadOnlyServiceInstance target
        ) {
            foreach(var factory in this.instanceClientFactories) {
                var client = factory.TryCreateClient(target);
                if (client != null) {
                    return client;
                }
            }            
            return null;
        }

        // -----------------------------------------------------------------------------
        // Registration 

        private async Task<bool> RefreshFabricRegistration(
            ILogSession log,
            IEnumerable<IReadOnlyDescriptor> addDependencies = null
        ) { 
            IRegistryClient registryClient = ExternalRegistryClient;
            if (registryClient == null) {
                registryClient = AttemptCreateRegistryClient(log);
            } 
            if (registryClient == null) {
                return false;
            }

            var request = new RefreshRegistrationRequest {
                FabricId = FabricId,
                UpdateStatus = new FabricStatus {
                    FabricId = FabricId,
                    Health = HostEnvironment?.GetHealth(),
                    RunState = HostEnvironment?.HostRunState
                },
                UpdateServiceStatuses = HostEnvironment?.GetHostedServiceStatuses().ToList(),
                NewDependencies = addDependencies?.Select(d => d.Clone()).ToList()
            };
            var response = await registryClient.Refresh(request);

            if (!response.ImmediateSuccess) {
                log?.WarningAs("Could not complete registration refresh", code: MonitorCodes.RegistrationRefreshFailed);
                return false;
            }

            ResetTopologyMapForDependencies(response.ResponseBody);

            return true;
        }

        private void ResetTopologyMapForDependencies(IReadOnlyServiceTopologyMap map) {
            if (map?.Services != null) {
                IReadOnlyList<IReadOnlyDescriptor> refreshedDependencies = null;
                lock (this.dependencies) {
                    // the new list of dependencies will be the union of what we had already 
                    // combined with what we receive.  This essentially means that the registryServiceRegistration
                    // service can "push" new dependencies simply by returning them.
                    // If we somehow get more than one listed for the same service, take the highest
                    // version.
                    refreshedDependencies = 
                        (from d in this.dependencies.Union(map.Services.Select(s => s.Descriptor))
                        group d by Identifiers.GetServiceVersionLevelKey(d) into g
                        select 
                            (from dep in g 
                            orderby dep.UpdateLevel, dep.PatchLevel descending 
                            select dep)
                            .First()
                        ).ToArray();
                }
                this.dependencies = refreshedDependencies;

                lock (this.dependencyRegistrations) {
                    this.dependencyRegistrations.Clear();

                    foreach (var r in map.Services) {
                        AddCachedDependencyServiceRegistration(r);
                    }
                    this.cacheExpiration = map.CacheExpiration;
                }

                this.callRouters = new ConcurrentDictionary<string, IServiceCallRouter>();
            }
        }

        private void AddCachedDependencyServiceRegistration(IReadOnlyServiceRegistration serviceRegistration) {
            this.dependencyRegistrations[Identifiers.GetServiceRegistrationKey(serviceRegistration)] = serviceRegistration;
        }

        private void AddCachedCallRouter(IReadOnlyDescriptor target, IServiceCallRouter router) {
            if (router != null) {
                this.callRouters[Identifiers.GetServiceFullRegistrationKey(target)] = router;
            }
        }

        private IReadOnlyServiceRegistration TryGetDependencyServiceRegistration(IReadOnlyDescriptor descriptor) {
            // Note that the registryServiceRegistration service will determine what instances meet the requirement
            // for a dependency and return _all_ of them.  So we don't have to worry about it. 
            this.dependencyRegistrations.TryGetValue(
                Identifiers.GetServiceFullRegistrationKey(descriptor),
                out var registration
            );
            return registration;
        }

        private IServiceCallRouter TryGetCachedServiceCallRouter(
            IReadOnlyDescriptor target
        ) {
            this.callRouters.TryGetValue(
                Identifiers.GetServiceFullRegistrationKey(target),
                out var cached
            );
            return cached;
        }

        private IReadOnlyServiceTopologyMap CreateLocalTopologyMap(
            IReadOnlyFabricRegistration registration,
            bool temporary
        ) {
            IReadOnlyServiceRegistration originalRegistryServiceRegistration = TryGetDependencyServiceRegistration(ServiceConstants.Managed.StandardServices.Registry.Descriptor);

            // Use the locally hosted services 
            var localDependencyRegistrations = 
                (registration?.HostedServices ?? Array.Empty<IReadOnlyServiceRegistration>())
                .Where(sr => !sr.Descriptor.IsSameService(ServiceConstants.Managed.StandardServices.Registry.Descriptor))
                .Select(sr => sr.Clone())
                .ToList();

            if (originalRegistryServiceRegistration != null) {
                localDependencyRegistrations.Add(originalRegistryServiceRegistration.Clone());                
            }

            return new ServiceTopologyMap {
                CacheExpiration = temporary ? DateTime.UtcNow.AddSeconds(ReAttemptDependencyResolutionTimeoutSeconds) : DateTime.MaxValue,
                Services = localDependencyRegistrations
            };
        }
    }
}

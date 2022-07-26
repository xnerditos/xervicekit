using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Services.Registry;
using XKit.Lib.Common.Utility;
using XKit.Lib.Host.DefaultBaseClasses;

namespace Samples.RegistryService; 
public partial class RegistryOperation : ServiceOperation<IRegistryService>, IRegistryApi {

    public RegistryOperation(
        ServiceOperationContext context
    ) : base(
        context
    ) { }

    // =====================================================================
    // IRegistrySvc
    // =====================================================================
    async Task<ServiceCallResult<ServiceTopologyMap>> IRegistryApi.Register(FabricRegistration request) 
        => await RunServiceCall(
            request,
            operationAction: Register
        );


    async Task<ServiceCallResult<ServiceTopologyMap>> IRegistryApi.Refresh(RefreshRegistrationRequest request) 
        => await RunServiceCall(
            request,
            operationAction: Refresh
        );        

    async Task<ServiceCallResult> IRegistryApi.Unregister(UnregisterRequest request) 
        => await RunServiceCall(
            request,
            operationAction: Unregister
        );

    // =====================================================================
    // private workers
    // =====================================================================

    private Task<ServiceTopologyMap> Register(FabricRegistration registration) {
        try {
            Service.AddRegistration(registration);
            return Task.FromResult(CreateTopologyMap(registration.Dependencies));
        } catch (Exception ex) {
            return Task.FromException<ServiceTopologyMap>(ex);
        }
    }

    private Task<ServiceTopologyMap> Refresh(RefreshRegistrationRequest request) {
        try {
            return Task.FromResult(CreateTopologyMap());
        } catch (Exception ex) {
            return Task.FromException<ServiceTopologyMap>(ex);
        }
    }

    private Task Unregister(UnregisterRequest request) {
        Service.RemoveRegistration(request.FabricId);
        return Task.CompletedTask; 
    }

    private ServiceTopologyMap CreateTopologyMap(IEnumerable<Descriptor> needed = null) {

        var dependencies = 
            Service.GetRegistrations()
            .Where(r => r.HostedServices != null)
            .SelectMany(r => r.HostedServices)
            .DistinctBy(s => Identifiers.GetServiceFullRegistrationKey(s.Descriptor))
            .Select(d => d.Clone())
            .ToList();
        // if we need a specific list of dependencies, filter the list for them.
        if (needed != null) {
            var neededKeys = 
                needed.Select(d => Identifiers.GetServiceVersionLevelKey(d))
                .ToHashSet();
            dependencies =
                dependencies
                .Where(s => neededKeys.Contains(Identifiers.GetServiceVersionLevelKey(s.Descriptor)))
                .ToList();
        }
        return new ServiceTopologyMap {
            CacheExpiration = DateTime.MaxValue,
            Services = dependencies
        };
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Services.Registry;
using XKit.Lib.Host.DefaultBaseClasses;

namespace XKit.Lib.Testing.TestRegistrySvc {

    public partial class RegistrySvcOperation : ServiceOperation<IRegistrySvcService>, IRegistryApi {

        public RegistrySvcOperation(
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
        // workers
        // =====================================================================

        private Task<ServiceTopologyMap> Register(FabricRegistration registration) {
            try {
                Service.SetTestRegistration(registration);
                return Task.FromResult(CreateTopologyMap());
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
            return Task.CompletedTask;     // Does nothing 
        }

        // =====================================================================
        // private
        // =====================================================================

        private ServiceTopologyMap CreateTopologyMap() {

            var dependencies = Service.GetTestRegistration().HostedServices.Select(
                hs => hs.Clone()
            ).ToList();

            return new ServiceTopologyMap {
                CacheExpiration = DateTime.MaxValue,
                Services = dependencies
            };
        }
   }
}

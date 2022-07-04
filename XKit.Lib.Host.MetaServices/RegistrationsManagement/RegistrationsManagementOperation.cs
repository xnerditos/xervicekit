using System.Threading.Tasks;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;
using XKit.Lib.Host.DefaultBaseClasses;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.MetaServices;

namespace XKit.Lib.Host.MetaServices.RegistrationsManagement {

    internal class RegistrationsManagementOperation : ServiceOperation<IRegistrationsManagementMetaService>, IRegistrationsManagementApi {

        public RegistrationsManagementOperation(
            ServiceOperationContext context
        ) :base(context) { }

        // =====================================================================
        // IRegistrationManagementOperation
        // =====================================================================

        async Task<ServiceCallResult> IRegistrationsManagementApi.ResetTopologyMap(
            ServiceTopologyMap map
        ) => await RunServiceCall(
            map, 
            operationAction: async (r) => await Connector.ForceResetTopologyMap(r)
        ); 

        async Task<ServiceCallResult> IRegistrationsManagementApi.TriggerRefresh() 
            => await RunServiceCall(
                operationAction: async () => { await Connector.Refresh(Log); }
            );
    }
}

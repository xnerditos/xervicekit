using System.Threading.Tasks;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Host;
using Mapster;

namespace XKit.Lib.Connector.Protocols.Direct {

    /// <summary>
    /// This class basically serves as a "faux-client" for direct connection.  
    /// It just serves to route the request.
    /// 
    /// There are a few classes that look kind of alike, and chain together.  This one is on the
    /// "client" side of the equation.
    /// </summary>
    internal class DirectLocalClient : IInstanceClient {

        private readonly IServiceBase Service;
        private readonly ServiceInstance InstanceInfo;

        public DirectLocalClient(
            IReadOnlyServiceInstance instanceInfo,
            IServiceBase service
        ) {
            this.InstanceInfo = instanceInfo.Clone();
            this.Service = service;
        }

        // =====================================================================
        // IInstanceClient
        // =====================================================================

        IReadOnlyServiceInstance IInstanceClient.Instance => this.InstanceInfo;

        async Task<ServiceCallResult> IInstanceClient.ExecuteCall(
            ServiceCallRequest request
        ) {
            var result = await Service.ExecuteCall(request);
            return UpdateInstance(result);
        }

        // =====================================================================
        // private
        // =====================================================================

        private ServiceCallResult UpdateInstance(ServiceCallResult resp) {
            
            if (resp.ServiceStatus != null) {
                InstanceInfo.Status.TryUpdate(resp.ServiceStatus);
            } else {
                switch(resp.OperationStatus) {
                    case LogResultStatusEnum.NoAction_ServiceUnavailable:
                    case LogResultStatusEnum.NoAction_Timeout:
                        InstanceInfo.Status.TryUpdate(AvailabilityEnum.UnavailableRetryLater, null);
                        break;
                    case LogResultStatusEnum.Success:
                    case LogResultStatusEnum.PartialSuccess:
                        if (resp.ServiceStatus != null) {
                            this.InstanceInfo.Status.TryUpdate(resp.ServiceStatus.Availability, resp.ServiceStatus.Health);
                        }
                        break;
                }
            }

            return resp;
        }

        private ServiceCallResult<T> UpdateInstance<T>(
            ServiceCallResult<T> resp
        ) where T : class {
            return (ServiceCallResult<T>)UpdateInstance((ServiceCallResult)resp);
        }
    }
}

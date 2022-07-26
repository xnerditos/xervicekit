using XKit.Lib.Connector.Service;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Client;
using XKit.Lib.Common.Log;
using System.Threading.Tasks;
using Samples.SampleService.V1.ServiceApiEntities;

namespace Samples.SampleService.V1.Client;

public interface ISampleServiceClient : ISampleServiceApi, IServiceClient<ISampleServiceApi> {}

public class SampleServiceClient : ServiceClientBase<ISampleServiceApi>, ISampleServiceClient {

    public SampleServiceClient(
        ILogSession log,
        IFabricConnector connector,
        ServiceCallTypeParameters defaultCallTypeParameters
    ) : base(
        Constants.ServiceDescriptor,
        log,
        connector,
        defaultCallTypeParameters
    ) {
    }

    // TODO:  Fill in service operation calls following this model 
    async Task<ServiceCallResult<SampleResponse>> ISampleServiceApi.SampleCall(
        SampleRequest request
    ) => await ExecuteCall<SampleRequest, SampleResponse>(request);

    /* Fill in new service operation calls here ^^  _
                                                   /.\
                                                    |
                                                    | */
}

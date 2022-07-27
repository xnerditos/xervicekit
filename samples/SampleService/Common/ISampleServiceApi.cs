using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using Samples.SampleService.V1.ServiceApiEntities;

namespace Samples.SampleService.V1; 

public interface ISampleServiceApi : IServiceApi {

    Task<ServiceCallResult<SampleResponse>> SampleCall(SampleRequest request);

    /* Define new service operation calls here ^^   _
                                                   /.\
                                                    |
                                                    | */
}

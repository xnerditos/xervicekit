using System.Threading.Tasks;
using Samples.SampleService.V1.ServiceApiEntities;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;

namespace Samples.SampleService.V1; 

// TODO:  Delete this if there are no events

public interface SampleServiceEvents : IServiceEvents {

    Task<ServiceCallResult> SampleEvent(SampleEventMessage message);

    /* Define new service events here ^^   _
                                                   /.\
                                                    |
                                                    | */
}

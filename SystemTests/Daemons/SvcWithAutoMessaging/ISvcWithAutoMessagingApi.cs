using System.Threading.Tasks;
using SystemTests.Daemons.SvcWithAutoMessaging.Entities;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;

namespace SystemTests.Daemons.SvcWithAutoMessaging {

    public interface ISvcWithAutoMessagingApi : IServiceApi {

        // Task<ServiceApiResult<TestValueResponse>> GetTestValue(
        //     TestValueRequest request
        // );
    }
}
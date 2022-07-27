using System.Threading.Tasks;
using XKit.Lib.Common.Host;

namespace Samples.SampleService.V1 {
    public interface ISampleServiceService : IServiceBase {

        Task<SampleServiceConfig> GetConfig(SampleServiceConfig defaultValue = default(SampleServiceConfig));
    }
}

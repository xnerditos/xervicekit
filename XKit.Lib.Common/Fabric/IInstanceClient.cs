using System.Threading.Tasks;
using XKit.Lib.Common.Registration;

namespace XKit.Lib.Common.Fabric {
    public interface IInstanceClient {
        IReadOnlyServiceInstance Instance { get; }

        Task<ServiceCallResult> ExecuteCall(ServiceCallRequest request);
    }
}
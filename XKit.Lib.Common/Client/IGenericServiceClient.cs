using System.Collections.Generic;
using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;

namespace XKit.Lib.Common.Client {

    /// <summary>
    /// Base interface for a service client 
    /// </summary>
    public interface IGenericServiceClient { 

        Task<IReadOnlyList<ServiceCallResult<TResponseBody>>> ExecuteCall<TRequestBody, TResponseBody>(
            string operationName,
            TRequestBody requestBody,
            ServiceCallTypeParameters callTypeParameters = null,
            ILogSession log = null,
            ServiceClientErrorHandling? errorHandling = null,
            ServiceCallPolicy policy = null
        ) where TRequestBody : class where TResponseBody : class;

        Task<IReadOnlyList<ServiceCallResult<TResponseBody>>> ExecuteCall<TResponseBody>(
            string operationName,
            ServiceCallTypeParameters callTypeParameters = null,
            ILogSession log = null,
            ServiceClientErrorHandling? errorHandling = null,
            string requestJsonPayload = null,
            ServiceCallPolicy policy = null
        ) where TResponseBody : class;

        Task<IReadOnlyList<ServiceCallResult>> ExecuteCall<TRequestBody>(
            string operationName,
            TRequestBody requestBody,
            ServiceCallTypeParameters callTypeParameters = null,
            ILogSession log = null,
            ServiceClientErrorHandling? errorHandling = null,
            ServiceCallPolicy policy = null
        ) where TRequestBody : class;

        Task<IReadOnlyList<ServiceCallResult>> ExecuteCall(
            string operationName,
            ServiceCallTypeParameters callTypeParameters = null,
            ILogSession log = null,
            ServiceClientErrorHandling? errorHandling = null,
            string requestJsonPayload = null,
            ServiceCallPolicy policy = null
        );
    }
}

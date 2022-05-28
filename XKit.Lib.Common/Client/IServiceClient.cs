using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Log;

namespace XKit.Lib.Common.Client {

    /// <summary>
    /// Base interface for a service client 
    /// </summary>
    public interface IServiceClient<TCallInterface> where TCallInterface : IServiceApi { 

        Task<IReadOnlyList<ServiceCallResult<TResponseBody>>> ExecuteWith<TRequestBody, TResponseBody>(
            Expression<Func<TCallInterface, Task<ServiceCallResult<TResponseBody>>>> expression,
            TRequestBody requestBody,
            ServiceCallTypeParameters callTypeParameters = null,
            ILogSession log = null,
            IServiceCallRouter callRouter = null,
            ServiceClientErrorHandling? errorHandling = null,
            string requestJsonPayload = null
        ) where TRequestBody : class where TResponseBody : class;

        Task<IReadOnlyList<ServiceCallResult<TResponseBody>>> ExecuteWith<TResponseBody>(
            Expression<Func<TCallInterface, Task<ServiceCallResult<TResponseBody>>>> expression,
            ServiceCallTypeParameters callTypeParameters = null,
            ILogSession log = null,
            IServiceCallRouter callRouter = null,
            ServiceClientErrorHandling? errorHandling = null
        ) where TResponseBody : class;

        Task<IReadOnlyList<ServiceCallResult>> ExecuteWith<TRequestBody>(
            Expression<Func<TCallInterface, Task<ServiceCallResult>>> expression,
            TRequestBody requestBody,
            ServiceCallTypeParameters callTypeParameters = null,
            ILogSession log = null,
            IServiceCallRouter callRouter = null,
            ServiceClientErrorHandling? errorHandling = null,
            string requestJsonPayload = null
        ) where TRequestBody : class;

        Task<IReadOnlyList<ServiceCallResult>> ExecuteWith(
            Expression<Func<TCallInterface, Task<ServiceCallResult>>> expression,
            ServiceCallTypeParameters callTypeParameters = null,
            ILogSession log = null,
            IServiceCallRouter callRouter = null,
            ServiceClientErrorHandling? errorHandling = null
        );
    }
}

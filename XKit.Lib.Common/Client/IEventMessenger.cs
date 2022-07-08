using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;

namespace XKit.Lib.Common.Client {

    public interface IEventMessenger<TCallInterface> where TCallInterface : IServiceCallable { 

        Task<Guid?> RaiseEvent(
            Expression<Func<TCallInterface, Task<ServiceCallResult>>> expression
        );

        Task<Guid?> RaiseEvent<TPayload>(
            Expression<Func<TCallInterface, Task<ServiceCallResult>>> expression,
            TPayload payload
        ) where TPayload : class, new();

        Task<Guid?> RaiseEvent(
            string eventName,
            string payloadJson = null
        );

        Task<Guid?> RaiseEvent<TPayload>(
            string eventName,
            TPayload payload
        ) where TPayload : class;
    }
}

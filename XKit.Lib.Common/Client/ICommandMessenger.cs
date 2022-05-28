using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;

namespace XKit.Lib.Common.Client {

    public interface ICommandMessenger<TCallInterface> where TCallInterface : IServiceCommands { 

        Task<Guid?> IssueCommand(
            Expression<Func<TCallInterface, Task<ServiceCallResult>>> expression
        );

        Task<Guid?> IssueCommand<TPayload>(
            Expression<Func<TCallInterface, Task<ServiceCallResult>>> expression,
            TPayload payload
        ) where TPayload : class, new();


        Task<Guid?> IssueCommand(
            string command,
            string payloadJson
        );

        Task<IReadOnlyList<ServiceCallResult>> GetResults(
            Guid messageId,
            float? waitForCompleteTimeoutSeconds = 5
        );
    }
}

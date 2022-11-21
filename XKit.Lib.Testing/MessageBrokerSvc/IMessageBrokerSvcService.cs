using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Services.MessageBroker;

namespace XKit.Lib.Testing.TestMessageBrokerSvc {
    public interface IMessageBrokerSvcService : IManagedService, IServiceBase {

        bool WaitForMessage(
            Guid messageId, 
            int timeoutMs = 400
        );

        bool WaitForMessage(
            string messageName, 
            int timeoutMs = 400
        );
        
        bool WaitForMessage<TCallInterface>(
            string methodName,
            int timeoutMs = 400
        ) where TCallInterface : IServiceCallable; 

        bool WasMessageSent(
            string messageName
        );

        bool WasMessageSent<TCallInterface>(
            string methodName
        ) where TCallInterface : IServiceCallable;

        void InterceptMessage<TCallInterface>(
            string methodName,
            Action<FabricMessage, IEnumerable<ServiceCallResult>> action
        );

        void ClearSentMessages();
        
        IReadOnlyList<Subscription> GetSubscriptions(string messageTypeName); 

        void AddSubscription(
            Subscription subscription
        );

        void RemoveSubscription(
            Subscription subscription
        );

        Task<IReadOnlyList<ServiceCallResult>> SendMessage(
            FabricMessage message,
            ILogSession log
        );
        
        Task<IReadOnlyList<ServiceCallResult>> SendMessage<TCallInterface, TMessage>(
            string methodName,
            TMessage payload,
            Guid messageId = default(Guid)
        ) where TCallInterface : IServiceCallable;

        Dictionary<string, IReadOnlyList<ServiceCallResult>> GetSentMessagesResults();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XKit.Lib.Common.Client;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Services.MessageBroker;
using XKit.Lib.Common.Utility;
using XKit.Lib.Common.Utility.Extensions;
using XKit.Lib.Connector.Service;
using XKit.Lib.Host.DefaultBaseClasses;

namespace XKit.Lib.Testing.TestMessageBrokerSvc {

    public class MessageBrokerSvcService : ManagedService<MessageBrokerSvcOperation>, IMessageBrokerSvcService {

        private static readonly IReadOnlyDescriptor descriptor = XKit.Lib.Common.Services.StandardConstants.Managed.StandardServices.MessageBroker.Descriptor;

        private List<Subscription> subscriptions = new();
        private readonly Dictionary<string, IReadOnlyList<ServiceCallResult>> messageResults = new();
        private readonly Dictionary<string, Action<FabricMessage, IEnumerable<ServiceCallResult>>> messageInterceptors = new();

        // =====================================================================
        // overrides
        // =====================================================================

        protected override IReadOnlyDescriptor Descriptor => descriptor;

        protected override IEnumerable<IReadOnlyDescriptor> Dependencies => Array.Empty<IReadOnlyDescriptor>();

        protected override IReadOnlyServiceCallPolicy CallPolicy => DefaultPolicy;

        protected override string ConfigurationDocumentIdentifier => Identifiers.GetServiceVersionLevelKey(descriptor);

        // =====================================================================
        // construction
        // =====================================================================

        public MessageBrokerSvcService(
            IXkitHostEnvironment hostEnv
        ) : base(hostEnv) { }

        // =====================================================================
        // IMessageBrokerSvcService
        // =====================================================================

        public bool WaitForMessage(
            Guid messageId, 
            int timeoutMs = 400
        ) {
            DateTime start = DateTime.UtcNow;
            while(!WasMessageSent(messageId) && (DateTime.UtcNow - start).TotalMilliseconds < timeoutMs) {
                System.Threading.Thread.Sleep(10);
            }
            return WasMessageSent(messageId);
        }

        public bool WaitForMessage(
            string messageName, 
            int timeoutMs = 400
        ) {
            DateTime start = DateTime.UtcNow;
            while(!WasMessageSent(messageName) && (DateTime.UtcNow - start).TotalMilliseconds < timeoutMs) {
                System.Threading.Thread.Sleep(10);
            }
            return WasMessageSent(messageName);
        }

        public bool WaitForMessage<TCallInterface>(
            string methodName,
            int timeoutMs = 400
        ) where TCallInterface : IServiceCallable 
            => WaitForMessage(typeof(TCallInterface).Name + "." + methodName, timeoutMs);

        public bool WasMessageSent(
            string messageName
        ) {
            lock(this.messageResults) {
                return this.messageResults.ContainsKey(messageName);
            }
        }

        public bool WasMessageSent(
            Guid messageId
        ) {
            lock(this.messageResults) {
                return this.messageResults.ContainsKey(messageId.ToString());
            }
        }

        public bool WasMessageSent<TCallInterface>(
            string methodName
        ) where TCallInterface : IServiceCallable 
            => WasMessageSent(typeof(TCallInterface).Name + "." + methodName);

        public void ClearSentMessages() {
            lock(this.messageResults) {
                this.messageResults.Clear();
            }
        }

        public void InterceptMessage<TCallInterface>(
            string methodName,
            Action<FabricMessage, IEnumerable<ServiceCallResult>> action
        ) {
            lock (messageInterceptors) {
                messageInterceptors[$"{typeof(TCallInterface).Name}.{methodName}"] = action;
            }
        }

        public IReadOnlyList<Subscription> GetSubscriptions() {
            lock(this.subscriptions) {
                return subscriptions.ToArray();
            }
        }

        public void AddSubscription(
            Subscription subscription
        ) {
            lock(subscriptions) {
                subscriptions.Add(subscription);
            }
        }

        public void RemoveSubscription(
            Subscription subscription
        ) {
            lock(subscriptions) {
                subscriptions = 
                    subscriptions
                    .Where(
                        s => s.MessageTypeName != subscription.MessageTypeName &&
                             Identifiers.GetServiceVersionLevelKey(s.Recipient) != Identifiers.GetServiceVersionLevelKey(subscription.Recipient)
                    ).ToList();
            }
        }

        public async Task<IReadOnlyList<ServiceCallResult>> SendMessage(
            FabricMessage message,
            ILogSession log
        ) {
            var allResults = new List<ServiceCallResult>();

            foreach(var subscription in GetSubscriptions()) {
                var messageNameParsed = message.MessageTypeName.Split('.');

                var client = ClientFactory.Factory.CreateGenericServiceClient(
                    descriptor: subscription.Recipient,
                    operationInterfaceName: messageNameParsed.Length < 2 ? null : messageNameParsed[0],
                    log,
                    Connector,
                    ServiceCallTypeParameters.SyncResult(),
                    errorHandling: subscription.ErrorHandling.GetValueOrDefault(ServiceClientErrorHandling.LogWarning),
                    targetHostId: subscription.RecipientHostId
                );

                var results = await client.ExecuteCall(
                    operationName: messageNameParsed[^1],
                    requestJsonPayload: message.JsonPayload,
                    policy: subscription.Policy?.Clone()
                );

                allResults.AddRange(results);
            }

            lock(this.messageResults) {
                messageResults[message.MessageTypeName] = allResults;
                messageResults[message.MessageId.ToString()] = allResults;
            }

            lock(this.messageInterceptors) {
                messageInterceptors.TryGetValue(message.MessageTypeName, out var action);
                action?.Invoke(message, allResults);
            }

            return allResults;
        }

        public Task<IReadOnlyList<ServiceCallResult>> SendMessage<TCallInterface, TMessage>(
            string methodName,
            TMessage payload,
            Guid messageId = default
        ) where TCallInterface : IServiceCallable => SendMessage(
            new FabricMessage {
                MessageId = messageId == default ? Guid.NewGuid() : messageId,
                MessageTypeName = typeof(TCallInterface).Name + "." + methodName,
                JsonPayload = Json.ToJson<TMessage>(payload)
            },
            null
        );

        public Dictionary<string, IReadOnlyList<ServiceCallResult>> GetSentMessagesResults() {
            lock (this.messageResults) {
                return new Dictionary<string, IReadOnlyList<ServiceCallResult>>(this.messageResults);
            }
        }
    }
}

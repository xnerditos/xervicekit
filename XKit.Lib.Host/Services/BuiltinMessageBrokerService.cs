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
using XKit.Lib.Common.Utility.Extensions;
using XKit.Lib.Connector.Service;
using XKit.Lib.Host.DefaultBaseClasses;

namespace XKit.Lib.Host.Services {

    public interface IBuiltinMessageBrokerService : IManagedService, IServiceBase {

        bool WaitForMessage(
            Guid messageId, 
            int timeoutMs = 400
        );

        void AddSubscription(
            Subscription subscription
        );

        Task<IReadOnlyList<ServiceCallResult>> SendMessage(
            FabricMessage message,
            ILogSession log
        );       
    }

    // -------------------------------------------------------------------------
    public class BuiltinMessageBrokerService : ManagedService<BuiltinMessageBrokerOperation>, IBuiltinMessageBrokerService {

        private readonly List<Subscription> subscriptions = new();
        private readonly Dictionary<string, IReadOnlyList<ServiceCallResult>> messageResults = new();

        protected override IReadOnlyDescriptor Descriptor => XKit.Lib.Common.Services.StandardConstants.Managed.StandardServices.MessageBroker.Descriptor;

        public BuiltinMessageBrokerService(
            IXkitHostEnvironment hostEnv
        ) : base(hostEnv) { }

        // =====================================================================
        // IMessageBrokerSvcService
        // =====================================================================

        bool IBuiltinMessageBrokerService.WaitForMessage(
            Guid messageId, 
            int timeoutMs
        ) {
            DateTime start = DateTime.UtcNow;
            while(!WasMessageSent(messageId) && (DateTime.UtcNow - start).TotalMilliseconds < timeoutMs) {
                System.Threading.Thread.Sleep(10);
            }
            return WasMessageSent(messageId);
        }

        void IBuiltinMessageBrokerService.AddSubscription(
            Subscription subscription
        ) {
            lock(subscriptions) {
                subscriptions.Add(subscription);
            }
        }

        async Task<IReadOnlyList<ServiceCallResult>> IBuiltinMessageBrokerService.SendMessage(
            FabricMessage message,
            ILogSession log
        ) {
            var allResults = new List<ServiceCallResult>();
            var messageNameParsed = message.MessageTypeName.Split('.');
            var operationInterfaceName = messageNameParsed.Length < 2 ? null : messageNameParsed[0];
            var messageOperationName = messageNameParsed[^1];

            foreach(var subscription in GetSubscriptions(message.MessageTypeName)) {

                var client = ClientFactory.Factory.CreateGenericServiceClient(
                    descriptor: subscription.Recipient,
                    operationInterfaceName: operationInterfaceName,
                    log: log,
                    HostEnvironment.Connector,
                    ServiceCallTypeParameters.SyncResult(),
                    errorHandling: subscription.ErrorHandling.GetValueOrDefault(ServiceClientErrorHandling.LogWarning),
                    targetHostId: subscription.RecipientHostId
                );

                var results = await client.ExecuteCall(
                    operationName: messageOperationName,
                    requestJsonPayload: message.JsonPayload,
                    policy: subscription.Policy?.Clone()
                );

                allResults.AddRange(results);
            }

            lock(this.messageResults) {
                messageResults[message.MessageTypeName] = allResults;
                messageResults[message.MessageId.ToString()] = allResults;
            }
            return allResults;
        }

        private bool WasMessageSent(
            Guid messageId
        ) {
            lock(this.messageResults) {
                return this.messageResults.ContainsKey(messageId.ToString());
            }
        }

        private IReadOnlyList<Subscription> GetSubscriptions(string messageTypeName) {
            lock(this.subscriptions) {
                return 
                    subscriptions
                    .Where(s => s.MessageTypeName == messageTypeName)
                    .ToArray();
            }
        }
    }

    // -------------------------------------------------------------------------
    public class BuiltinMessageBrokerOperation : ServiceOperation<IBuiltinMessageBrokerService>, IMessageBrokerApi {

        public BuiltinMessageBrokerOperation(
            ServiceOperationContext context
        ) : base(
            context
        ) { }

        Task<ServiceCallResult> IMessageBrokerApi.IssueCommand(
            FabricMessage request
        ) => RunServiceCall(
                request,
                operationAction: (r) => {
                    Service.SendMessage(r, Log);
                    return Task.CompletedTask;
                }
            );

        Task<ServiceCallResult> IMessageBrokerApi.RaiseEvent(
            FabricMessage request
        ) => RunServiceCall(
                request,
                operationAction: (r) => {
                    Service.SendMessage(r, Log);
                    return Task.CompletedTask;
                }
            );

        Task<ServiceCallResult> IMessageBrokerApi.Subscribe(
            SubscribeRequest request
        ) => RunServiceCall(
                request,
                operationAction: (r) => {
                    request.Subscriptions.ForEach(s => Service.AddSubscription(s));
                    return Task.CompletedTask;
                }
            );

        Task<ServiceCallResult<WaitOnMessageResponse>> IMessageBrokerApi.WaitOnMessage(
            WaitOnMessageRequest request
        ) => RunServiceCall(
                request,
                operationAction: (r) => {
                    var b = Service.WaitForMessage(r.MessageId, (int)(r.WaitTimeoutSeconds * 1000f));
                    return Task.FromResult(new WaitOnMessageResponse {
                        Complete = b,
                        MessageId = r.MessageId
                    });
                }
            );
    }
}

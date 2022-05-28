using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using XKit.Lib.Common.Client;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.ObjectInstantiation;
using XKit.Lib.Common.Services.MessageBroker;
using XKit.Lib.Common.Utility.Extensions;
using XKit.Lib.Connector.Fabric;

namespace XKit.Lib.Connector.Service {

    public class CommandMessenger<TCallInterface> : ICommandMessenger<TCallInterface>
        where TCallInterface : IServiceCommands {

        private readonly IMessageBrokerClient Broker;

        // =====================================================================
        // construction
        // =====================================================================

        public CommandMessenger(
            ILogSession log,
            IDependencyConnector connector,
            IMessageBrokerClient broker = null
        ) {
            // IInProcessGlobalObjectRepository igor = null;
            // if (broker == null || clientParameters == null) {
            //     igor = InProcessGlobalObjectRepositoryFactory.CreateSingleton();
            // }

            this.Broker = broker ?? new InternalMessageBrokerClient(
                log,
                connector
            );
        }

        async Task<IReadOnlyList<ServiceCallResult>> ICommandMessenger<TCallInterface>.GetResults(
            Guid messageId, 
            float? waitSeconds
        ) {
            var waitResult = await Broker.WaitOnMessage(new WaitOnMessageRequest {
                MessageId = messageId,
                WaitTimeoutSeconds = waitSeconds
            });
            if (waitResult.HasError) {
                return null;
            }
            return waitResult.ResponseBody.Results;
        }

        async Task<Guid?> ICommandMessenger<TCallInterface>.IssueCommand(
            Expression<Func<TCallInterface, Task<ServiceCallResult>>> expression
        ) {
            var id = Guid.NewGuid();
            return (await Broker.IssueCommand(
                new FabricMessage {
                    MessageId = id,
                    MessageTypeName = $"{typeof(TCallInterface).Name}.{((MethodCallExpression)expression.Body).Method.Name}"
                }
            )).HasError ? (Guid?)null : id;
        } 

        async Task<Guid?> ICommandMessenger<TCallInterface>.IssueCommand(
            string command,
            string payloadJson
        ) {
            var id = Guid.NewGuid();
            return (await Broker.IssueCommand(
                new FabricMessage {
                    MessageId = id,
                    JsonPayload = payloadJson,
                    MessageTypeName = $"{typeof(TCallInterface).Name}.{command}",
                }
            )).HasError ? (Guid?)null : id;
        } 

        async Task<Guid?> ICommandMessenger<TCallInterface>.IssueCommand<TPayload>(
            Expression<Func<TCallInterface, Task<ServiceCallResult>>> expression,
            TPayload payload
        ) {
            var id = Guid.NewGuid();
            return (await Broker.IssueCommand(
                new FabricMessage {
                    MessageId = id,
                    JsonPayload = Json.To<TPayload>(payload),
                    MessageTypeName = $"{typeof(TCallInterface).Name}.{((MethodCallExpression)expression.Body).Method.Name}"
                }
            )).HasError ? (Guid?)null : id;
        }        
    }
}

using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using XKit.Lib.Common.Client;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Services.MessageBroker;
using XKit.Lib.Common.Utility.Extensions;
using XKit.Lib.Connector.Fabric;

namespace XKit.Lib.Connector.Service {

    public class EventMessenger<TCallInterface> : IEventMessenger<TCallInterface>
        where TCallInterface : IServiceEvents {

        private readonly IMessageBrokerApi Broker;

        // =====================================================================
        // construction
        // =====================================================================

        public EventMessenger(
            ILogSession log,
            IDependencyConnector connector,
            IMessageBrokerApi broker = null
        ) {
            //IObjectRepository igor = null;
            // if (broker == null || clientParameters == null) {
            //     igor = ObjectRepositoryFactory.CreateSingleton();
            // }

            this.Broker = broker ?? new InternalMessageBrokerClient(
                log,
                connector
            );
        }

        async Task<Guid?> IEventMessenger<TCallInterface>.RaiseEvent(
            Expression<Func<TCallInterface, Task<ServiceCallResult>>> expression
        ) {
            var id = Guid.NewGuid();
            return (await Broker.RaiseEvent(
                new FabricMessage {
                    MessageId = id,
                    MessageTypeName = $"{typeof(TCallInterface).Name}.{((MethodCallExpression)expression.Body).Method.Name}"
                }
            )).HasError ? (Guid?)null : id;
        } 

        async Task<Guid?> IEventMessenger<TCallInterface>.RaiseEvent(
            string eventName,
            string payloadJson
        ) {
            var id = Guid.NewGuid();
            return (await Broker.RaiseEvent(
                new FabricMessage {
                    MessageId = id,
                    JsonPayload = payloadJson,
                    MessageTypeName = $"{typeof(TCallInterface).Name}.{eventName}"
                }
            )).HasError ? (Guid?)null : id;
        } 

        async Task<Guid?> IEventMessenger<TCallInterface>.RaiseEvent<TPayload>(
            Expression<Func<TCallInterface, Task<ServiceCallResult>>> expression,
            TPayload payload
        ) {
            var id = Guid.NewGuid();
            return (await Broker.RaiseEvent(
                new FabricMessage {
                    MessageId = id,
                    JsonPayload = Json.To<TPayload>(payload),
                    MessageTypeName = $"{typeof(TCallInterface).Name}.{((MethodCallExpression)expression.Body).Method.Name}"
                }
            )).HasError ? (Guid?)null : id;
        } 
    }
}

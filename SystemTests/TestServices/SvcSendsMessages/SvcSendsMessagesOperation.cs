using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Connector.Service;
using XKit.Lib.Host.DefaultBaseClasses;

namespace TestServices.SvcSendsMessages; 

public partial class SvcSendsMessagesOperation : ServiceOperation<ISvcSendsMessagesService>, ISvcSendsMessagesApi {

    public SvcSendsMessagesOperation(
        ServiceOperationContext context
    ) : base(context) { }

    Task<ServiceCallResult> ISvcSendsMessagesApi.RaisesEvent1(Message request) {
        return RunServiceCall(
            request,
            operationAction: (r) => {
                var messenger = ClientFactory.Factory.CreateEventMessengerClient<TestEvents>(
                    Log,
                    DependencyConnector
                );
                messenger.RaiseEvent<Message>(
                    (x) => x.Event1(null), 
                    r
                );
                return Task.CompletedTask;
            }
        );
    }

    Task<ServiceCallResult> ISvcSendsMessagesApi.RaisesEvent2(Message request) {
        return RunServiceCall(
            request,
            operationAction: (r) => {
                var messenger = ClientFactory.Factory.CreateEventMessengerClient<TestEvents>(
                    Log,
                    DependencyConnector
                );
                messenger.RaiseEvent<Message>(
                    x => x.Event2(null), 
                    r
                );
                return Task.CompletedTask;
            }
        );
    }

    Task<ServiceCallResult> ISvcSendsMessagesApi.IssuesCommand1(Message request) {
        return RunServiceCall(
            request,
            operationAction: (r) => {
                var messenger = ClientFactory.Factory.CreateCommandMessengerClient<TestCommands>(
                    Log,
                    DependencyConnector
                );
                messenger.IssueCommand<Message>(
                    x => x.Command1(null), 
                    r
                );
                return Task.CompletedTask;
            }
        );
    }

    Task<ServiceCallResult> ISvcSendsMessagesApi.IssuesCommand2(Message request) {
        return RunServiceCall(
            request,
            operationAction: (r) => {
                var messenger = ClientFactory.Factory.CreateCommandMessengerClient<TestCommands>(
                    Log,
                    DependencyConnector
                );
                messenger.IssueCommand<Message>(
                    x => x.Command2(null), 
                    r
                );
                return Task.CompletedTask;
            }
        );
    }

    async Task<ServiceCallResult> ISvcSendsMessagesApi.IssuesCommand1AndWaitsForFinish(Message request) {
        return await RunServiceCall(
            request,
            operationAction: async (r) => {
                var messenger = ClientFactory.Factory.CreateCommandMessengerClient<TestCommands>(
                    Log,
                    DependencyConnector
                );
                var messageGuid = await messenger.IssueCommand<Message>(
                    x => x.Command1(null), 
                    r
                );
                await messenger.GetResults(messageGuid.Value, 1);
                return Task.CompletedTask;
            }
        );
    }
}

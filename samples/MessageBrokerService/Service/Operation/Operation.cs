using System.Threading.Tasks;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Utility;
using XKit.Lib.Host.DefaultBaseClasses;
using Samples.MessageBroker.Engine;
using XKit.Lib.Common.Services.MessageBroker;

namespace Samples.MessageBroker; 
public partial class MessageBrokerOperation : ServiceOperation<IMessageBrokerService>, IMessageBrokerApi, IServiceOperation {

    private readonly SetOnceOrThrow<IMessageEngine> engine = new();
    private IMessageEngine Engine => engine.Value;

    public MessageBrokerOperation(
        ServiceOperationContext context
    ) : base(
        context
    ) { }

    // =====================================================================
    // general workers
    // =====================================================================

    // protected override async Task<bool> InitServiceOperation() {
    //     this.config = await Service.GetConfig();
    //     return true;
    // }

    protected Task<bool> PreOperationAction(bool writeable) {
        engine.Value = Service.GetMessageEngine();
        return Task.FromResult(true);
    }
}

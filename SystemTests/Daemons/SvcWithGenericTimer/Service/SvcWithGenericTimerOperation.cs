
using XKit.Lib.Common.Host;
using XKit.Lib.Host.DefaultBaseClasses;

namespace SystemTests.Daemons.SvcWithGenericTimer.Service {

    public partial class SvcWithGenericTimerOperation : ServiceOperation<ISvcWithGenericTimerService>, ISvcWithGenericTimerApi {

        public SvcWithGenericTimerOperation(
            ServiceOperationContext context
        ) : base(context) { }
    }
}
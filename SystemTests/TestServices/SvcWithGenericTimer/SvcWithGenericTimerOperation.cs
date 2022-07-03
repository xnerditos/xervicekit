
using XKit.Lib.Common.Host;
using XKit.Lib.Host.DefaultBaseClasses;

namespace TestServices.SvcWithGenericTimer {

    public partial class SvcWithGenericTimerOperation : ServiceOperation<ISvcWithGenericTimerService>, ISvcWithGenericTimerApi {

        public SvcWithGenericTimerOperation(
            ServiceOperationContext context
        ) : base(context) { }
    }
}

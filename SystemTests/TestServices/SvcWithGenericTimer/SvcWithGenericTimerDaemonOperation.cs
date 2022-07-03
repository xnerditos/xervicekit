using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Log;
using XKit.Lib.Host.DefaultBaseClasses;

namespace TestServices.SvcWithGenericTimer {

    public partial class SvcWithGenericTimerDaemonOperation 
        : ServiceDaemonOperation<object> {

        public static volatile uint LastMessageTickValue = 0;

        public SvcWithGenericTimerDaemonOperation(
            ServiceDaemonOperationContext context
        ) : base(context) { }

        protected async override Task DoOperationLogic(object _) {
            await Task.Delay(10);
            string name = nameof(SvcWithGenericTimerDaemonOperation);
            var threadId = Thread.CurrentThread.ManagedThreadId;
            Debug.WriteLine($"From {name}.  Thread id {threadId}");
        }
    }
}

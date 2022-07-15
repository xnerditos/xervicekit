using System.Diagnostics;
using XKit.Lib.Common.Host;
using XKit.Lib.Host.Services;

namespace TestServices.SvcWithGenericTimer {

    public partial class SvcWithGenericTimerDaemonOperation 
        : GenericTimerDaemonOperation {

        public static volatile uint LastMessageTickValue = 0;

        public SvcWithGenericTimerDaemonOperation(
            ServiceDaemonOperationContext context
        ) : base(context) { }

        protected override async Task DoTimerOperation() {
            await Task.Delay(10);
            string name = nameof(SvcWithGenericTimerDaemonOperation);
            var threadId = Thread.CurrentThread.ManagedThreadId;
            Debug.WriteLine($"FromJson {name}.  Thread id {threadId}");
        }
    }
}

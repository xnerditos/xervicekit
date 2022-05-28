using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SystemTests.Daemons.SvcWithAutoMessaging.Entities;
using XKit.Lib.Common.Host;
using XKit.Lib.Host.DefaultBaseClasses;

namespace SystemTests.Daemons.SvcWithAutoMessaging.Service {

    public partial class SvcWithAutoMessagingDaemonOperation 
        : ServiceDaemonOperation<DaemonMessage> {

        public static volatile uint LastMessageTickValue = 0;

        public SvcWithAutoMessagingDaemonOperation(
            ServiceDaemonOperationContext context
        ) : base(context) { }

        protected async override Task DoOperationLogic(DaemonMessage message) {
            await Task.Delay(10);
            LastMessageTickValue = message.Ticks;
            string name = nameof(SvcWithAutoMessagingDaemon);
            var threadId = Thread.CurrentThread.ManagedThreadId;
            Debug.WriteLine($"From {name}.  Thread id {threadId}: {message.Message}");
        }
    }
}
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using XKit.Lib.Common.Host;
using XKit.Lib.Host.DefaultBaseClasses;

namespace TestServices.SvcWithAutoMessaging {

    public interface ISvcWithAutoMessagingService : IManagedService, IServiceBase { }

    public interface ISvcWithAutoMessagingDaemon : IServiceDaemon<DaemonMessage> { } 

    public class SvcWithAutoMessagingDaemon : ServiceDaemon<SvcWithAutoMessagingDaemonOperation, DaemonMessage, SvcWithAutoMessagingDaemonTimerOperation>, ISvcWithAutoMessagingDaemon {
        protected override string Name => "AutoMessagingDaemon";

        public SvcWithAutoMessagingDaemon() 
            : base(XKit.Lib.Log.LogSessionFactory.Factory) {
            this.DefaultTimerPeriodMilliseconds = 90;
            this.EnableTimerEvent = true;
        }

        protected override void OnTimerEvent() {
            var nowTicks = (uint)(DateTime.Now.Ticks & 0xffffffff);

             var threadId = System.Environment.CurrentManagedThreadId;
             PostMessage(new DaemonMessage {
                Ticks = nowTicks,
                Message = $"Clock ticks are currently {nowTicks}.  Main thread is {threadId}"
            });
        }
    }

    public class SvcWithAutoMessagingDaemonOperation 
        : ServiceDaemonOperation<DaemonMessage> {

        private static volatile uint lastMessageTickValue = 0;
        public static uint LastMessageTickValue => lastMessageTickValue;

        public SvcWithAutoMessagingDaemonOperation(
            ServiceDaemonOperationContext context
        ) : base(context) { }

        protected override async Task DoOperationLogic(DaemonMessage message) {
            await Task.Delay(10);
            lastMessageTickValue = message.Ticks;
            string name = nameof(SvcWithAutoMessagingDaemon);
            var threadId = System.Environment.CurrentManagedThreadId;
            Debug.WriteLine($"FromJson {name}.  Thread id {threadId}: {message.Message}");
        }
    }

    public class SvcWithAutoMessagingDaemonTimerOperation : ServiceDaemonOperation {
        public SvcWithAutoMessagingDaemonTimerOperation(
            ServiceDaemonOperationContext context
        ) : base(context) { }

        protected override Task DoOperationLogic() {
            var nowTicks = (uint)(DateTime.Now.Ticks & 0xffffffff);

            var threadId = System.Environment.CurrentManagedThreadId;
            ((ISvcWithAutoMessagingDaemon) Daemon).PostMessage(new DaemonMessage {
                Ticks = nowTicks,
                Message = $"Clock ticks are currently {nowTicks}.  Main thread is {threadId}"
            });
            return Task.CompletedTask;
        }
    }
}

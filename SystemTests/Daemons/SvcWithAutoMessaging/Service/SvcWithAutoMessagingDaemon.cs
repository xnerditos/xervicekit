using System;
using System.Threading;
using SystemTests.Daemons.SvcWithAutoMessaging.Entities;
using XKit.Lib.Common.Host;
using XKit.Lib.Host.DefaultBaseClasses;
using XKit.Lib.LocalLog;

namespace SystemTests.Daemons.SvcWithAutoMessaging.Service {

    public interface ISvcWithAutoMessagingDaemon : IServiceDaemon { } 

    public class SvcWithAutoMessagingDaemon : ServiceDaemon<SvcWithAutoMessagingDaemonOperation, DaemonMessage>, ISvcWithAutoMessagingDaemon {
        protected override string Name => "AutoMessagingDaemon";

        public SvcWithAutoMessagingDaemon() 
            : base(XKit.Lib.LocalLog.LogSessionFactory.Factory) {
            this.WakeDelayMillisecondsWhenNoMessagesWaiting = 200;
            this.WakeDelayMillisecondsWhenMessagesWaiting = 200;
            this.EnableEnqueueEvent = true;
            this.AutoPulseActive = true;
        }
        protected override IServiceDaemonOperation<DaemonMessage> CreateDaemonOperation(ServiceDaemonOperationContext context) 
            => new SvcWithAutoMessagingDaemonOperation(context);

        protected override void OnEnqueueEvent() {
            var nowTicks = (uint)(DateTime.Now.Ticks & 0xffffffff);

             var threadId = Thread.CurrentThread.ManagedThreadId;
             PostMessage(new DaemonMessage {
                Ticks = nowTicks,
                Message = $"Clock ticks are currently {nowTicks}.  Main thread is {threadId}"
            });
        }

        protected override TimeSpan? OnDetermineEnqueueEventPeriod() {
            return TimeSpan.FromMilliseconds(1);
        }
    }
}

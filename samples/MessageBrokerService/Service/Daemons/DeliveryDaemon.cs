using System;
using System.Collections;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Utility.Threading;
using XKit.Lib.Host.DefaultBaseClasses;

namespace Samples.MessageBroker.Daemons; 

public interface IDeliveryDaemon : IServiceDaemon<DeliveryDaemonMessage> {
    void AddWakeTime(DateTime? wakeTime);
}

public class DeliveryDaemon : ServiceDaemon<DeliveryDaemonOperation, DeliveryDaemonMessage>, IDeliveryDaemon {

    const int DEFAULT_TIMER_PERIOD_MILLISECONDS = 1000;
    const int DEFAULT_TIMER_PERIOD_NOTHING_WAITING_MILLISECONDS = 20000;
    const int DEFAULT_MAX_CONCURRENT_MESSAGES = 8;

    private new IMessageBrokerService Service => base.Service as IMessageBrokerService;
    private readonly SortedList RetryTimes = SortedList.Synchronized(new SortedList());
    private MessageBrokerConfig.DeliveryDaemonType config;

    public DeliveryDaemon(ILogSessionFactory logFactory) 
        : base(logFactory) {
        this.EnableTimerEvent = true;
        this.MaxConcurrentMessages = DEFAULT_MAX_CONCURRENT_MESSAGES;
    }

    void IDeliveryDaemon.AddWakeTime(DateTime? wakeTime) {
        if (wakeTime == null) {
            return;
        }
        RetryTimes.Add(wakeTime.Value.Ticks, wakeTime);
    }

    // =====================================================================
    // Overrides
    // =====================================================================
    protected override string Name => nameof(DeliveryDaemon);

    protected override uint? OnDetermineTimerEventPeriod() {
        lock (RetryTimes) {
            int delay;
            if (RetryTimes.Count <= 0) {
                if (config?.SanityCheckEnabled == false) {
                    return null;
                }
                delay = config?.SanityCheckDelayMs ?? DEFAULT_TIMER_PERIOD_NOTHING_WAITING_MILLISECONDS;
            } else {
                var dt = (DateTime)RetryTimes.GetByIndex(0);
                RetryTimes.RemoveAt(0);

                // return 50 milliseconds more than the indicate time to guarantee processing.
                delay = (int)(dt - DateTime.UtcNow).Add(TimeSpan.FromMilliseconds(50)).TotalMilliseconds;
                if (delay < 0) { delay = 0; }
            }
            return (uint) delay;
        }
    }

    protected override void OnDaemonStarting() {
        SetConfigurableParameters();
    }

    protected override void OnEnvironmentChange() {
        SetConfigurableParameters();
    }

    private void SetConfigurableParameters() {
        config = TaskUtil.RunAsyncAsSync(() => Service?.GetConfig())?.DeliveryDaemon;
        MaxConcurrentMessages = config?.MaxConcurrentMessages ?? DEFAULT_MAX_CONCURRENT_MESSAGES;
    }
}

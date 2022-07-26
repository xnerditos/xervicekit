using System.Diagnostics;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Host.DefaultBaseClasses;

namespace TestServices.SvcWithAutoMessaging; 

public interface ISvcWithAutoMessagingService : IManagedService, IServiceBase { }

public interface ISvcWithAutoMessagingDaemon : IServiceDaemon<DaemonMessage> { } 

public class SvcWithAutoMessagingDaemon : ServiceDaemon<SvcWithAutoMessagingDaemonOperation, DaemonMessage>, ISvcWithAutoMessagingDaemon {
    protected override string Name => "AutoMessagingDaemon";

    public SvcWithAutoMessagingDaemon() 
        : base(XKit.Lib.Log.LogSessionFactory.Factory) {
        EnableTimerEvent = true;
        MaxConcurrentMessages = 4;
    }

    protected override uint? OnDetermineTimerEventPeriod() => 90;
}

public class SvcWithAutoMessagingDaemonOperation 
    : ServiceDaemonOperation<DaemonMessage> {

    private static volatile uint lastMessageTickValue = 0;
    public static uint LastMessageTickValue => lastMessageTickValue;

    public SvcWithAutoMessagingDaemonOperation(
        ServiceDaemonOperationContext context
    ) : base(context) { }

    protected override async Task<OperationResult> DoMessageOperation(DaemonMessage message) {
        await Task.Delay(10);
        lastMessageTickValue = message.Ticks;
        string name = nameof(SvcWithAutoMessagingDaemon);
        var threadId = System.Environment.CurrentManagedThreadId;
        Debug.WriteLine($"FromJson {name}.  Thread id {threadId}: {message.Message}");
        return ResultSuccess();
    }

    protected override Task DoTimerOperation() {
        var nowTicks = (uint)(DateTime.Now.Ticks & 0xffffffff);

        var threadId = System.Environment.CurrentManagedThreadId;
        ((ISvcWithAutoMessagingDaemon) Daemon).PostMessage(new DaemonMessage {
            Ticks = nowTicks,
            Message = $"Clock ticks are currently {nowTicks}.  Main thread is {threadId}"
        });
        Daemon.SetTimerDelay(900);
        return Task.CompletedTask;
    }
}


using Samples.SampleService.V1.ServiceApiEntities;
using XKit.Lib.Common.Host;
using XKit.Lib.Host.DefaultBaseClasses;

namespace Samples.SampleService.V1; 

public interface ISampleDaemon : IServiceDaemon { } 

public class SampleDaemon : ServiceDaemon<SampleDaemonOperation, SampleDaemonMessage>, ISampleDaemon {
    protected override string Name => "SampleDaemon";

    public SampleDaemon() 
        : base(XKit.Lib.Log.LogSessionFactory.Factory) {
        this.EnableTimerEvent = true;
    }

    protected override uint? OnDetermineTimerEventPeriod() {
        return 5000;  // run timer event every 5 seconds.
    }
}

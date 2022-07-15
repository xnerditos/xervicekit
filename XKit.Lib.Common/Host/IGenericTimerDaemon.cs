namespace XKit.Lib.Common.Host {

    public interface IGenericTimerDaemonOperation : IServiceDaemonOperation<object> { }

    public interface IGenericTimerDaemon : IServiceDaemon<object> { 
        void SetTimerDelay(uint? milliseconds);
        void SetTimerEnabled(bool enabled);
    } 
}

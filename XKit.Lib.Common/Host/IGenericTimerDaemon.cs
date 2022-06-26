namespace XKit.Lib.Common.Host {
    public interface IGenericTimerDaemon : IServiceDaemon<object> { 
        void SetTimerDelay(uint milliseconds);
        void SetNextEventReadyDelay(uint? milliseconds);
        void SetTimerEnabled(bool enabled);
    } 
}

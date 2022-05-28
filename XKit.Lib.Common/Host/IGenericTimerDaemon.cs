namespace XKit.Lib.Common.Host {
    public interface IGenericTimerDaemon : IServiceDaemon<object> { 
        void ManuallyPostNewTimerEvent();
        void SetTimerDelay(int? milliseconds);
        void SetNextEventReadyDelay(int? milliseconds);
        void SetTimerEnabled(bool? enabled);
    } 
}
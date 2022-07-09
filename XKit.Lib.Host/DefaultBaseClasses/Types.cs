using XKit.Lib.Common.Log;

namespace XKit.Lib.Host.DefaultBaseClasses {

    public delegate void OnEnvironmentChangeDelegate(ILogSession log);
    public delegate void OnServiceStartDelegate(ILogSession log);
    public delegate void OnHostStartCompleteDelegate(ILogSession log);
    public delegate void OnServiceStopDelegate(ILogSession log);
    public delegate void OnServicePauseDelegate(ILogSession log);
    public delegate void OnServiceResumeDelegate(ILogSession log);
}

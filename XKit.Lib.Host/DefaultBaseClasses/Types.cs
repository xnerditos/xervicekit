using System;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Log;

namespace XKit.Lib.Host.DefaultBaseClasses {

    public delegate void OnEnvironmentChangeDelegate();
    public delegate void OnServiceStartDelegate();
    public delegate void OnHostStartCompleteDelegate();
    public delegate void OnServiceStopDelegate();
    public delegate void OnServicePauseDelegate();
    public delegate void OnServiceResumeDelegate();
}

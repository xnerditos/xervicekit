using XKit.Lib.Common.Log;

namespace XKit.Lib.Common.Host {
    
    /// <summary>
    /// An IManagedService is not the controller, but rather the 
    /// core implementation of the service or an object that communicates with it.
    /// Generally, the implementing object will provide this interface to work with the
    /// host, and another to provide functionality to the controller that receives the
    /// requests.
    /// </summary>
    public interface IManagedService : IServiceBase {
        void PauseService();
        void ResumeService();
    }
}

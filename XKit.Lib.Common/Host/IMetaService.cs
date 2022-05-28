using System.Collections.Generic;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;

namespace XKit.Lib.Common.Host {
    
    /// <summary>
    /// An IMetaService is the core implementation of a meta service or an object 
    /// that communicates with it.  Meta services run in relation to controlling or
    /// communicating with the host itself.  They are not discoverable as externally
    /// services in relation to the registry.  They are also not individually controllable
    /// apart from the host itself.  They generally are considered to have higher 
    /// "privileges" as compared to individual services that should have no knowledge
    /// of the host.
    ///  
    /// Generally, the implementing object will provide this interface to work with the
    /// host, and another to provide functionality to the controller that receives the
    /// requests.
    /// </summary>
    public interface IMetaService : IServiceBase {
        
        string CapabilityKeyName { get; } 
    }
}
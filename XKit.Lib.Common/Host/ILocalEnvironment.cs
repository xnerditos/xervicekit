using System.Collections.Generic;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;

namespace XKit.Lib.Common.Host {

    public interface ILocalEnvironment {
        string FabricId { get; }
        ILogSessionFactory LogSessionFactory { get; }
        IEnumerable<IReadOnlyDescriptor> GetDependencies();
        IDependencyConnector DependencyConnector { get; }
        IHostEnvironment HostEnvironment { get; }
   }
}

using System.Collections.Generic;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;

namespace XKit.Lib.Testing.TestRegistrySvc {
    public interface IRegistrySvcService : IManagedService, IServiceBase {

        IReadOnlyList<IReadOnlyFabricRegistration> GetTestRegistrations();

        void AddTestRegistration(FabricRegistration registration);
    }
}

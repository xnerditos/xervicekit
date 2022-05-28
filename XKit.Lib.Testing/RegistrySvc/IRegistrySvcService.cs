using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;

namespace XKit.Lib.Testing.TestRegistrySvc {
    public interface IRegistrySvcService : IManagedService, IServiceBase {

        IReadOnlyFabricRegistration GetTestRegistration();

        void SetTestRegistration(FabricRegistration registration);
    }
}

using System.Collections.Generic;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;

namespace Samples.RegistryService; 

public interface IRegistryService : IManagedService {

    IReadOnlyList<IReadOnlyFabricRegistration> GetRegistrations();

    void AddRegistration(FabricRegistration registration);

    void RemoveRegistration(string fabricId);
}

using System.Collections.Generic;
using XKit.Lib.Common.Registration;
using XKit.Lib.Host.DefaultBaseClasses;
using XKit.Lib.Common.Host;
using System;
using System.Linq;
using System.Collections.Concurrent;

namespace Samples.RegistryService; 
public class RegistryService : ManagedService<RegistryOperation>, IRegistryService {

    // NOTE:  The Registry service, being one of a handful of "Platform" services, 
    //        uses the standard descriptor that the framework knows about.
	private static readonly IReadOnlyDescriptor descriptor = XKit.Lib.Common.Services.StandardConstants.Managed.StandardServices.Registry.Descriptor;
		
    private readonly ConcurrentDictionary<string, FabricRegistration> registrations = new();

    // =====================================================================
    // overrides
    // =====================================================================

    protected override IReadOnlyDescriptor Descriptor => descriptor;

    // =====================================================================
    // construction
    // =====================================================================

    public RegistryService(
        IXKitHostEnvironment hostEnv
    ) : base(hostEnv) { }

    // =====================================================================
    // IRegistryService
    // =====================================================================

    IReadOnlyList<IReadOnlyFabricRegistration> IRegistryService.GetRegistrations() 
        => registrations.Values.ToArray();

    void IRegistryService.AddRegistration(FabricRegistration registration) 
        => registrations.TryAdd(registration.FabricId, registration);

    void IRegistryService.RemoveRegistration(string fabricId)
        => registrations.TryRemove(fabricId, out var _);
}

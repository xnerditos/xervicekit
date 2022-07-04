using System.Collections.Generic;
using XKit.Lib.Common.Registration;
using XKit.Lib.Host.DefaultBaseClasses;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Utility;
using System;
using System.Linq;
using System.Collections.Concurrent;

namespace XKit.Lib.Testing.TestRegistrySvc {

	public class RegistrySvcService : ManagedService<RegistrySvcOperation>, IRegistrySvcService {

		private static readonly IReadOnlyDescriptor descriptor = XKit.Lib.Common.Services.StandardConstants.Managed.StandardServices.Registry.Descriptor;
		
        private readonly ConcurrentDictionary<string, FabricRegistration> testRegistrations = new();

		// =====================================================================
		// overrides
		// =====================================================================

		protected override IReadOnlyDescriptor Descriptor => descriptor;

        protected override IEnumerable<IReadOnlyDescriptor> Dependencies => Array.Empty<IReadOnlyDescriptor>();

        protected override IReadOnlyServiceCallPolicy CallPolicy => DefaultPolicy;
        
        protected override string ConfigurationDocumentIdentifier => Identifiers.GetServiceVersionLevelKey(descriptor);

		// =====================================================================
		// construction
		// =====================================================================

		public RegistrySvcService(
            IXkitHostEnvironment hostEnv
		) : base(hostEnv) { }

        protected override bool CanStartNewOperation() {
            switch(HostEnvironment.HostRunState)
            {
            case RunStateEnum.Inactive:
            case RunStateEnum.Paused:
            case RunStateEnum.Unknown:
            return false;
            }
            return this.RunState == RunStateEnum.Active;
        }    

		// =====================================================================
		// IRegistryService
		// =====================================================================

		IReadOnlyList<IReadOnlyFabricRegistration> IRegistrySvcService.GetTestRegistrations() 
            => testRegistrations.Values.ToArray();

        void IRegistrySvcService.AddTestRegistration(FabricRegistration registration) 
            => testRegistrations.TryAdd(registration.FabricId, registration);
	}
}

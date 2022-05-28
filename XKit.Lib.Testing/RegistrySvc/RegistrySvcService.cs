using System.Collections.Generic;
using XKit.Lib.Common.Registration;
using XKit.Lib.Host.DefaultBaseClasses;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Utility;

namespace XKit.Lib.Testing.TestRegistrySvc {

	public class RegistrySvcService : ManagedService<RegistrySvcOperation>, IRegistrySvcService {

		private static readonly IReadOnlyDescriptor descriptor = XKit.Lib.Common.Services.StandardConstants.Managed.StandardServices.Registry.Descriptor;
		
        private FabricRegistration testRegistration;

		// =====================================================================
		// overrides
		// =====================================================================

		protected override IReadOnlyDescriptor Descriptor => descriptor;

        protected override IEnumerable<IReadOnlyDescriptor> Dependencies => new IReadOnlyDescriptor[0];

        protected override IReadOnlyServiceCallPolicy CallPolicy => DefaultPolicy;
        
        protected override string ConfigurationDocumentIdentifier => Identifiers.GetServiceVersionLevelKey(descriptor);

		// =====================================================================
		// construction
		// =====================================================================

		public RegistrySvcService(
            ILocalEnvironment localFabric
		) : base(localFabric) { }

        protected override bool CanStartNewOperation() {
            switch(HostEnvironment.HostRunState)
            {
            case RunStateEnum.Inactive:
            case RunStateEnum.Paused:
            case RunStateEnum.Unknown:
            return false;
            // For the registry, we specifically allow operations during shutdown
            // case RunStateEnum.ShuttingDown:
            }
            return this.RunState == RunStateEnum.Active;
        }    

		// =====================================================================
		// IRegistryService
		// =====================================================================

		IReadOnlyFabricRegistration IRegistrySvcService.GetTestRegistration()
            => testRegistration;

        void IRegistrySvcService.SetTestRegistration(FabricRegistration registration) 
            => testRegistration ??= registration;
	}
}

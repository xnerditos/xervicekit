using System.Collections.Generic;
using XKit.Lib.Common.Registration;
using XKit.Lib.Host.DefaultBaseClasses;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Utility;

namespace SystemTests.ServiceCalls.SvcWithDependency2.Service {

	public class SvcWithDependency2Service 
        : ManagedService<SvcWithDependency2Operation>, ISvcWithDependency2Service {

		private static readonly IReadOnlyDescriptor descriptor = Constants.ServiceDescriptor;
		
		// =====================================================================
		// overrides
		// =====================================================================

		protected override IReadOnlyDescriptor Descriptor => descriptor;

        protected override IEnumerable<IReadOnlyDescriptor> Dependencies => System.Array.Empty<IReadOnlyDescriptor>();

        protected override IReadOnlyServiceCallPolicy CallPolicy => DefaultPolicy;

        protected override string ConfigurationDocumentIdentifier => Identifiers.GetServiceVersionLevelKey(descriptor);

		// =====================================================================
		// construction
		// =====================================================================

		public SvcWithDependency2Service(
            ILocalEnvironment localEnvironment
		) : base(localEnvironment) { }
	}
}

using System.Collections.Generic;
using XKit.Lib.Common.Registration;
using XKit.Lib.Host.DefaultBaseClasses;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Utility;

namespace SystemTests.ServiceCalls.SvcWithDependency1.Service {

	public class SvcWithDependency1Service 
        : ManagedService<SvcWithDependency1Operation>, ISvcWithDependency1Service {

		private static readonly IReadOnlyDescriptor descriptor = Constants.ServiceDescriptor;
		
		// =====================================================================
		// overrides
		// =====================================================================

		protected override IReadOnlyDescriptor Descriptor => descriptor;

        protected override IEnumerable<IReadOnlyDescriptor> Dependencies => new[] {
            SvcWithDependency2.Constants.ServiceDescriptor
        };

        protected override IReadOnlyServiceCallPolicy CallPolicy => DefaultPolicy;

        protected override string ConfigurationDocumentIdentifier => Identifiers.GetServiceVersionLevelKey(descriptor);

		// =====================================================================
		// construction
		// =====================================================================

		public SvcWithDependency1Service(
            ILocalEnvironment localEnvironment
		) : base(localEnvironment) {}
	}
}

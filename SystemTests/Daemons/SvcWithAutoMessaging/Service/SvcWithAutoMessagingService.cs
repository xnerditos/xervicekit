using XKit.Lib.Common.Registration;
using XKit.Lib.Host.DefaultBaseClasses;
using XKit.Lib.Common.Host;

namespace SystemTests.Daemons.SvcWithAutoMessaging.Service {

	public class SvcWithAutoMessagingService 
        : ManagedService<SvcWithAutoMessagingOperation>, ISvcWithAutoMessagingService {

		// =====================================================================
		// overrides
		// =====================================================================

		protected override IReadOnlyDescriptor Descriptor => Constants.ServiceDescriptor;

		// =====================================================================
		// construction
		// =====================================================================

		public SvcWithAutoMessagingService(
            ILocalEnvironment localEnvironment
		) : base(localEnvironment) { 
            AddDaemon(new SvcWithAutoMessagingDaemon());
		}
	}
}

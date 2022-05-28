using XKit.Lib.Common.Registration;
using XKit.Lib.Host.DefaultBaseClasses;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Host.Services;
using XKit.Lib.Testing;

namespace SystemTests.Daemons.SvcWithGenericTimer.Service {

	public class SvcWithGenericTimerService 
        : ManagedService<SvcWithGenericTimerOperation>, ISvcWithGenericTimerService {

		private static readonly IReadOnlyDescriptor descriptor = Constants.ServiceDescriptor;
		
		// =====================================================================
		// overrides
		// =====================================================================

		protected override IReadOnlyDescriptor Descriptor => descriptor;

		// =====================================================================
		// construction
		// =====================================================================

		public SvcWithGenericTimerService(
            ILocalEnvironment localEnvironment
		) : base(localEnvironment) { 
            AddDaemon(
                new GenericTimerDaemon<SvcWithGenericTimerDaemonOperation>(
                    logSessionFactory: TestHostHelper.LogSessionFactory,
                    timerDelayMilliseconds: 1000
            ));
        }	
    }
}

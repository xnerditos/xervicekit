using XKit.Lib.Common.Registration;
using XKit.Lib.Host.DefaultBaseClasses;
using XKit.Lib.Common.Host;
using XKit.Lib.Host.Services;

namespace TestServices.SvcWithGenericTimer {

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
            IXKitHostEnvironment hostEnv
		) : base(hostEnv) { 
            AddDaemon(
                new GenericTimerDaemon<SvcWithGenericTimerDaemonOperation>(
                    logSessionFactory: hostEnv.LogSessionFactory,
                    timerDelayMilliseconds: 1000
            ));
        }	
    }
}

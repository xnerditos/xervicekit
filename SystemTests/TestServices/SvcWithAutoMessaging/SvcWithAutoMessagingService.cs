using XKit.Lib.Common.Registration;
using XKit.Lib.Host.DefaultBaseClasses;
using XKit.Lib.Common.Host;

namespace TestServices.SvcWithAutoMessaging {

    public partial class SvcWithAutoMessagingOperation : ServiceOperation<ISvcWithAutoMessagingService>, ISvcWithAutoMessagingApi {

        public SvcWithAutoMessagingOperation(
            ServiceOperationContext context
        ) : base(context) { }
    }

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
            IXkitHostEnvironment hostEnv
		) : base(hostEnv) { 
            AddDaemon(new SvcWithAutoMessagingDaemon());
		}
	}
}

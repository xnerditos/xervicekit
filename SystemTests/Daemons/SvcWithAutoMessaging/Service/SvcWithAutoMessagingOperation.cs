using XKit.Lib.Common.Host;
using XKit.Lib.Host.DefaultBaseClasses;

namespace SystemTests.Daemons.SvcWithAutoMessaging.Service {

    public partial class SvcWithAutoMessagingOperation : ServiceOperation<ISvcWithAutoMessagingService>, ISvcWithAutoMessagingApi {

        public SvcWithAutoMessagingOperation(
            ServiceOperationContext context
        ) : base(context) { }
    }
}
using XKit.Lib.Common.Client;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;
using XKit.Lib.Connector.Service;

namespace TestServices.SvcWithGenericTimer {

    public class SvcWithGenericTimerClient : ServiceClientBase<ISvcWithGenericTimerApi>, ISvcWithGenericTimerApi {

        public SvcWithGenericTimerClient(
            IReadOnlyDescriptor dependency,
            ILogSession log,
            IDependencyConnector connector,
            ServiceCallTypeParameters defaultCallTypeParameters
        ) : base(
            dependency,
            log,
            connector,
            defaultCallTypeParameters
        ) { }

        // =====================================================================
        // overridables
        // =====================================================================

        protected override IReadOnlyDescriptor ServiceDescriptor => Constants.ServiceDescriptor;

    }
}

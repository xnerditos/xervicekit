using XKit.Lib.Common.Client;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;
using XKit.Lib.Connector.Service;

namespace SystemTests.Daemons.SvcWithAutoMessaging.Client {

    internal class SvcWithAutoMessagingClient : ServiceClientBase<ISvcWithAutoMessagingApi>, ISvcWithAutoMessagingApi {

        public SvcWithAutoMessagingClient(
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

        // =====================================================================
        // ISvcWithAutoMessaging
        // =====================================================================

        // async Task<ServiceApiResult<TestValueResponse>> ISvcWithAutoMessagingApi.GetTestValue(
        //     TestValueRequest request
        // ) => await ExecuteCall<TestValueRequest, TestValueResponse>(
        //     nameof(ISvcWithAutoMessagingApi.GetTestValue),
        //     request
        // );
    }
}

using System.Threading.Tasks;
using SystemTests.ServiceCalls.SvcSimple.Entities;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;
using XKit.Lib.Connector.Service;

namespace SystemTests.ServiceCalls.SvcSimple.Client {

    internal class SvcSimpleClient : ServiceClientBase<ISvcSimpleApi>, ISvcSimpleApi {

        public SvcSimpleClient(
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
        // ISvcSimple
        // =====================================================================

        async Task<ServiceCallResult<TestValueResponse>> ISvcSimpleApi.GetTestValueNoDependencies(
            TestValueRequest request
        ) => await ExecuteCall<TestValueRequest, TestValueResponse>(request);

        async Task<ServiceCallResult<TestValueResponse>> ISvcSimpleApi.Fails() 
            => await ExecuteCall<TestValueResponse>();

        async Task<ServiceCallResult> ISvcSimpleApi.ChangeStaticValue(TestValueRequest request) 
            => await ExecuteCall<TestValueRequest>(request);
    }
}

using System.Threading.Tasks;
using SystemTests.ServiceCalls.SvcGeneric.Entities;
using XKit.Lib.Common.Client;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;
using XKit.Lib.Connector.Service;

namespace SystemTests.ServiceCalls.SvcGeneric.Client {

    internal class SvcGenericClient : ServiceClientBase<ISvcGenericApi>, ISvcGenericApi {

        public SvcGenericClient(
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

        async Task<ServiceCallResult<TestValueResponse>> ISvcGenericApi.GetTestValueNoDependencies(
            TestValueRequest request
        ) => await ExecuteCall<TestValueRequest, TestValueResponse>(request);

        async Task<ServiceCallResult<TestValueResponse>> ISvcGenericApi.Fails() 
            => await ExecuteCall<TestValueResponse>();

        async Task<ServiceCallResult> ISvcGenericApi.ChangeStaticValue(TestValueRequest request) 
            => await ExecuteCall<TestValueRequest>(request);
    }
}

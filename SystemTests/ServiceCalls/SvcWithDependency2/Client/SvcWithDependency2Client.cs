using System.Threading.Tasks;
using SystemTests.ServiceCalls.SvcWithDependency2.Entities;
using XKit.Lib.Common.Client;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;
using XKit.Lib.Connector.Service;

namespace SystemTests.ServiceCalls.SvcWithDependency2.Client {

    internal class SvcWithDependency2Client : ServiceClientBase<ISvcWithDependency2>, ISvcWithDependency2 {

        public SvcWithDependency2Client(
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
        // ISvcWithDependency2
        // =====================================================================

        async Task<ServiceCallResult<TestValueResponse>> ISvcWithDependency2.GetTestValueWithDependency1Level(
            TestValueRequest request
        ) => await ExecuteCall<TestValueRequest, TestValueResponse>(request);

        async Task<ServiceCallResult> ISvcWithDependency2.ChangeStaticValueWithDependency1Level(
            TestValueRequest request
        ) => await ExecuteCall<TestValueRequest>(request);
    }
}

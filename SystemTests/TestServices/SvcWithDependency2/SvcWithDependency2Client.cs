using System.Threading.Tasks;
using XKit.Lib.Common.Client;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;
using XKit.Lib.Connector.Service;

namespace TestServices.SvcWithDependency2 {

    public class SvcWithDependency2Client : ServiceClientBase<ISvcWithDependency2>, ISvcWithDependency2 {

        public SvcWithDependency2Client(
            IReadOnlyDescriptor dependency,
            ILogSession log,
            IFabricConnector connector,
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

using System.Threading.Tasks;
using XKit.Lib.Common.Client;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;
using XKit.Lib.Connector.Service;

namespace TestServices.SvcWithDependency1; 

public class SvcWithDependency1Client : ServiceClientBase<ISvcWithDependency1>, ISvcWithDependency1 {

    public SvcWithDependency1Client(
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
    // ISvcWithDependency1
    // =====================================================================

    async Task<ServiceCallResult<TestValueResponse>> ISvcWithDependency1.GetTestValueWithDependency2Levels(
        TestValueRequest request
    ) => await ExecuteCall<TestValueRequest, TestValueResponse>(request);

    async Task<ServiceCallResult> ISvcWithDependency1.ChangeStaticValueWithDependency2Levels(
        TestValueRequest request
    ) => await ExecuteCall<TestValueRequest>(request);
}

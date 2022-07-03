using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Host.DefaultBaseClasses;
using FluentAssertions;

namespace TestServices.SvcWithDependency1; 

public partial class SvcWithDependency1Operation : ServiceOperation<ISvcWithDependency1Service>, ISvcWithDependency1 {

    public SvcWithDependency1Operation(
        ServiceOperationContext context
    ) : base(context) { }


    // =====================================================================
    // ISvcWithDependency1.GetTestValueWithDependency2Levels
    // =====================================================================
    async Task<ServiceCallResult<TestValueResponse>> ISvcWithDependency1.GetTestValueWithDependency2Levels(TestValueRequest request) {
        return await RunServiceCall(
            request,
            operationAction: async (r) => {
                var svcWithDep2 = SvcWithDependency2.SvcWithDependency2ClientFactory.CreateServiceClient(
                    log: Log,
                    connector: DependencyConnector,
                    defaultCallTypeParameters: ServiceCallTypeParameters.SyncResult()
                );
                var result = await svcWithDep2.GetTestValueWithDependency1Level(new SvcWithDependency2.TestValueRequest {
                    TheValue = r.TheValue
                });
                
                result?.ResponseBody.Should().NotBeNull();
                result.ImmediateSuccess.Should().BeTrue();

                return new TestValueResponse {
                    RandomValue = result.ResponseBody.RandomValue,
                    TheIncomingValue = result.ResponseBody.TheIncomingValue
                };
            }
        );
    }

    // =====================================================================
    // ISvcWithDependency1.ChangeStaticValueWithDependency2Levels
    // =====================================================================
    async Task<ServiceCallResult> ISvcWithDependency1.ChangeStaticValueWithDependency2Levels(TestValueRequest request) {
        return await RunServiceCall(
            request,
            operationAction: async (r) => {
                var svcWithDep2 = SvcWithDependency2.SvcWithDependency2ClientFactory.CreateServiceClient(
                    log: Log,
                    connector: DependencyConnector,
                    defaultCallTypeParameters: ServiceCallTypeParameters.FireAndForget()
                );
                var result = await svcWithDep2.ChangeStaticValueWithDependency1Level(new SvcWithDependency2.TestValueRequest {
                    TheValue = r.TheValue
                });
            }
        );
    }
}

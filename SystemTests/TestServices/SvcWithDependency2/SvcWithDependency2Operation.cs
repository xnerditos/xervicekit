using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Host.DefaultBaseClasses;
using FluentAssertions;

namespace TestServices.SvcWithDependency2 {

    public partial class SvcWithDependency2Operation : ServiceOperation<ISvcWithDependency2Service>, ISvcWithDependency2 {

        public SvcWithDependency2Operation(
            ServiceOperationContext context
        ) : base(context) { }


        // =====================================================================
        // GetTestValue
        // =====================================================================
        async Task<ServiceCallResult<TestValueResponse>> ISvcWithDependency2.GetTestValueWithDependency1Level(TestValueRequest request) {
            return await RunServiceCall(
                request,
                operationAction: async (r) => {
                    var svcSimple = SvcSimple.SvcSimpleClientFactory.CreateServiceClient(
                        log: Log,
                        connector: Connector
                    );
                    var result = await svcSimple.GetTestValueNoDependencies(new SvcSimple.TestValueRequest {
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

        async Task<ServiceCallResult> ISvcWithDependency2.ChangeStaticValueWithDependency1Level(TestValueRequest request) {
            return await RunServiceCall(
                request,
                operationAction: async (r) => {
                    var svcSimple = SvcSimple.SvcSimpleClientFactory.CreateServiceClient(
                        log: Log,
                        connector: Connector,
                        defaultCallTypeParameters: ServiceCallTypeParameters.FireAndForget()
                    );
                    var result = await svcSimple.ChangeStaticValue(new SvcSimple.TestValueRequest {
                        TheValue = r.TheValue
                    });
                }
            );
        }
    }
}

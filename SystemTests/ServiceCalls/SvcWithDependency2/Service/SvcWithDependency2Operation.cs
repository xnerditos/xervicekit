using System.Threading.Tasks;
using SystemTests.ServiceCalls.SvcWithDependency2.Entities;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Host.DefaultBaseClasses;
using FluentAssertions;

namespace SystemTests.ServiceCalls.SvcWithDependency2.Service {

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
                    var svcSimple = SvcSimple.Client.SvcSimpleClientFactory.CreateServiceClient(
                        log: Log,
                        connector: DependencyConnector
                    );
                    var result = await svcSimple.GetTestValueNoDependencies(new SvcSimple.Entities.TestValueRequest {
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
                    var svcSimple = SvcSimple.Client.SvcSimpleClientFactory.CreateServiceClient(
                        log: Log,
                        connector: DependencyConnector,
                        defaultCallTypeParameters: ServiceCallTypeParameters.FireAndForget()
                    );
                    var result = await svcSimple.ChangeStaticValue(new SvcSimple.Entities.TestValueRequest {
                        TheValue = r.TheValue
                    });
                }
            );
        }
    }
}

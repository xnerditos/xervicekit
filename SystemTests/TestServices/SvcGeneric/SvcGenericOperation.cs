using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Host.DefaultBaseClasses;
using SystemTests.ServiceCalls.Environment;
using XKit.Lib.Common.Log;

namespace TestServices.SvcGeneric; 

public partial class SvcGenericOperation : ServiceOperation, ISvcGenericApi {

    public SvcGenericOperation(
        ServiceOperationContext context
    ) : base(context) { }


    // =====================================================================
    // GetTestValueNoDependencies
    // =====================================================================
    async Task<ServiceCallResult<TestValueResponse>> ISvcGenericApi.GetTestValueNoDependencies(TestValueRequest request) {
        return await RunServiceCall(
            request,
            operationAction: (r) => Task.FromResult(new TestValueResponse {
                TheIncomingValue = r.TheValue,
                RandomValue = Guid.NewGuid().ToString()
            })
        );
    }

    async Task<ServiceCallResult<TestValueResponse>> ISvcGenericApi.Fails() {
        return await RunServiceCall(
            operationAction: () => {
                return Task.FromResult(
                    Result<TestValueResponse>(operationStatus: LogResultStatusEnum.NonRetriableError, resultData: null)
                );
            }
        );
    }

    async Task<ServiceCallResult> ISvcGenericApi.ChangeStaticValue(TestValueRequest request) {
        return await RunServiceCall(
            request,
            operationAction: (r) => {
                ValueHelper.SaveJsonTestData(r.TheValue);
                return Task.CompletedTask;
            }
        );
    }
}

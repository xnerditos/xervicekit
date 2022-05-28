using System;
using System.Threading.Tasks;
using SystemTests.ServiceCalls.SvcSimple.Entities;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Host.DefaultBaseClasses;
using SystemTests.ServiceCalls.Environment;
using XKit.Lib.Common.Log;

namespace SystemTests.ServiceCalls.SvcSimple.Service {

    public partial class SvcSimpleOperation : ServiceOperation<ISvcSimpleService>, ISvcSimpleApi {

        public SvcSimpleOperation(
            ServiceOperationContext context
        ) : base(context) { }


        // =====================================================================
        // GetTestValueNoDependencies
        // =====================================================================
        async Task<ServiceCallResult<TestValueResponse>> ISvcSimpleApi.GetTestValueNoDependencies(TestValueRequest request) {
            return await RunServiceCall(
                request,
                operationAction: (r) => {
                    var randomValue = Guid.NewGuid().ToString();
                    Trace("test trace", new { randomValue });
                    return Task.FromResult(new TestValueResponse {
                        TheIncomingValue = r.TheValue,
                        RandomValue = randomValue
                    });
                }
            );
        }

        async Task<ServiceCallResult<TestValueResponse>> ISvcSimpleApi.Fails() {
            return await RunServiceCall(
                operationAction: () => {
                    return Task.FromResult(
                        Result<TestValueResponse>(operationStatus: LogResultStatusEnum.NonRetriableError, resultData: null)
                    );
                }
            );
        }

        async Task<ServiceCallResult> ISvcSimpleApi.ChangeStaticValue(TestValueRequest request) {
            return await RunServiceCall(
                request,
                operationAction: (r) => {
                    ValueHelper.SaveJsonTestData(r.TheValue);
                    return Task.CompletedTask;
                }
            );
        }
    }
}

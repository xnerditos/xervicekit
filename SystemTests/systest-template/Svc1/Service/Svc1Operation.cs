using System;
using System.Threading.Tasks;
using SystemTests._NAMESPACE.Svc1.Entities;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Host.DefaultBaseClasses;

namespace SystemTests._NAMESPACE.Svc1.Service {

    public partial class Svc1Operation : ServiceOperation<ISvc1Service>, ISvc1Api {

        public Svc1Operation(
            ServiceOperationContext context
        ) : base(context) { }


        // ---------------------------------------------------------------------
        // ISvc1.GetTestValue
        // ---------------------------------------------------------------------
        async Task<ServiceCallResult<TestValueResponse>> ISvc1Api.GetTestValue(TestValueRequest request) {
            return await RunServiceCall(
                request,
                operationAction: (r) => Task.FromResult(new TestValueResponse {
                    TheIncomingValue = r.TheValue,
                    RandomValue = Guid.NewGuid().ToString()
                })
            );
        }
    }
}
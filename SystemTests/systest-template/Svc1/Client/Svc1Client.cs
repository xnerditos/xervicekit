using System.Threading.Tasks;
using SystemTests._NAMESPACE.Svc1.Entities;
using XKit.Lib.Common.Client;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;
using XKit.Lib.Connector.Service;

namespace SystemTests._NAMESPACE.Svc1.Client {

    internal class Svc1Client : ServiceClientBase<ISvc1Api>, ISvc1Api {

        public Svc1Client(
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
        // ISvc1
        // =====================================================================

        async Task<ServiceCallResult<TestValueResponse>> ISvc1Api.GetTestValue(
            TestValueRequest request
        ) => await ExecuteCall<TestValueRequest, TestValueResponse>(request);
    }
}

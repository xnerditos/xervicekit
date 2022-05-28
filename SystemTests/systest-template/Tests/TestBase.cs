using XKit.Lib.Common.Fabric;
using XKit.Lib.Host;
using XKit.Lib.Common.Client;
using XKit.Lib.Testing;

namespace SystemTests._NAMESPACE.Tests {

    public class TestBase {
        protected static IDependencyConnector DependencyConnector => 
            HostEnvironmentHelper.DependencyConnector;
        protected static string FabricId => 
            HostEnvironmentHelper.FabricConnector?.FabricId;

        protected static void ClassInit() {

            TestHostHelper.Initialize();

            TestHostHelper.AddService(Svc1.Service.Svc1ServiceFactory.Create());            

            TestHostHelper.StartHost();       
        }

        protected static void ClassTeardown() {
            TestHostHelper.DestroyHost();
        }

        protected static T CreateClient<T>(
            IServiceClientFactory<T> factory, 
            ServiceCallTypeParameters callTypeParameters = null
        ) {
            return factory.CreateServiceClient(
                log: TestHostHelper.Log,
                DependencyConnector,
                defaultCallTypeParameters: callTypeParameters ?? ServiceCallTypeParameters.SyncResult()
            );
        }
    }
}

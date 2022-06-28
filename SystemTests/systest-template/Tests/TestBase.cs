using XKit.Lib.Common.Fabric;
using XKit.Lib.Host;
using XKit.Lib.Common.Client;
using XKit.Lib.Testing;
using XKit.Lib.Common.Host;

namespace SystemTests._NAMESPACE.Tests {

    public class TestBase {
        protected static IDependencyConnector DependencyConnector => 
            TestHostHelper.HostEnvironmentHelper.DependencyConnector;
        protected static string FabricId => 
            TestHostHelper.HostEnvironmentHelper.FabricConnector?.FabricId;
        protected static IHostEnvironment HostEnvironment => TestHostHelper.HostEnvironmentHelper.Host;
        protected static ILocalEnvironment LocalEnvironment => TestHostHelper.HostEnvironmentHelper.Host;

        protected static void ClassInit() {

            TestHostHelper.Initialize();

            TestHostHelper.AddService(new Svc1.Service.Svc1Service(LocalEnvironment));            

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

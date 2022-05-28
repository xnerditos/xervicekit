using XKit.Lib.Common.Fabric;
using XKit.Lib.Host;
using XKit.Lib.Common.Client;
using XKit.Lib.Testing;

namespace SystemTests.Daemons.Tests {

    public class TestBase {

        protected static IDependencyConnector DependencyConnector => 
            HostEnvironmentHelper.DependencyConnector;
        protected static string FabricId => 
            HostEnvironmentHelper.FabricConnector.FabricId;

        protected static void ClassInit() {

            TestHostHelper.Initialize(useDaemonDebugMode: false);
            TestHostHelper.AddService(
                SvcWithAutoMessaging.Service.SvcWithAutoMessagingServiceFactory.Create()
            );
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
                connector: DependencyConnector,
                defaultCallTypeParameters: callTypeParameters ?? ServiceCallTypeParameters.SyncResult()
            );
        }
    }
}

using XKit.Lib.Common.Fabric;
using XKit.Lib.Host;
using XKit.Lib.Common.Client;
using XKit.Lib.Testing;
using XKit.Lib.Common.Host;
using System.Threading;

namespace SystemTests.Daemons.Tests {

    public class TestBase {
        protected readonly TestHostHelper TestHelper = new();
        protected HostEnvironmentHelper HostEnvironmentHelper => TestHelper.HostEnvironmentHelper;
        protected IFabricConnector Connector => HostEnvironmentHelper.Connector;
        protected string FabricId => HostEnvironmentHelper.Connector.FabricId;
        protected IXKitHostEnvironment HostEnvironment => HostEnvironmentHelper.Host;
        protected IManagedService AutoMessagingService { get; private set; }
        protected static uint LastMessageTickValue => TestServices.SvcWithAutoMessaging.SvcWithAutoMessagingDaemonOperation.LastMessageTickValue;

        protected static void Yield(int milliseconds = 400) {
            Thread.Sleep(milliseconds);
        }

        protected void TestInit() {

            TestHelper.InitializeLocalTestHost(useDaemonDebugMode: false);
            AutoMessagingService = TestHelper.AddService(
                TestServices.SvcWithAutoMessaging.SvcWithAutoMessagingServiceFactory.Create(HostEnvironment)
            );
            TestHelper.StartHost();
            Yield(1000);
        }

        protected void TestTeardown() {

            TestHelper.DestroyHost();
        }

        protected T CreateClient<T>(
            IServiceClientFactory<T> factory, 
            ServiceCallTypeParameters callTypeParameters = null
        ) {
            return factory.CreateServiceClient(
                log: TestHelper.Log,
                connector: Connector,
                defaultCallTypeParameters: callTypeParameters ?? ServiceCallTypeParameters.SyncResult()
            );
        }
    }
}

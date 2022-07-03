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
        protected IDependencyConnector DependencyConnector => HostEnvironmentHelper.DependencyConnector;
        protected string FabricId => HostEnvironmentHelper.FabricConnector.FabricId;
        protected IHostEnvironment HostEnvironment => HostEnvironmentHelper.Host;
        protected ILocalEnvironment LocalEnvironment => HostEnvironmentHelper.Host;
        protected IManagedService AutoMessagingService { get; private set; }
        protected uint LastMessageTickValue => TestServices.SvcWithAutoMessaging.SvcWithAutoMessagingDaemonOperation.LastMessageTickValue;

        protected static void Yield(int milliseconds = 200) {
            Thread.Sleep(milliseconds);
        }

        protected void TestInit() {

            TestHelper.InitializeLocalTestHost(useDaemonDebugMode: false);
            AutoMessagingService = TestHelper.AddService(
                TestServices.SvcWithAutoMessaging.SvcWithAutoMessagingServiceFactory.Create(LocalEnvironment)
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
                connector: DependencyConnector,
                defaultCallTypeParameters: callTypeParameters ?? ServiceCallTypeParameters.SyncResult()
            );
        }
    }
}

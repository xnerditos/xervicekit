using XKit.Lib.Common.Fabric;
using XKit.Lib.Host;
using XKit.Lib.Common.Client;
using XKit.Lib.Testing;
using XKit.Lib.Common.Host;
using System.Threading;

namespace SystemTests.Daemons.Tests {

    public class TestBase {
        private static readonly HostEnvironmentHelper HostEnvironmentHelper = TestHostHelper.HostEnvironmentHelper;
        protected static IDependencyConnector DependencyConnector => HostEnvironmentHelper.DependencyConnector;
        protected static string FabricId => HostEnvironmentHelper.FabricConnector.FabricId;
        protected static IHostEnvironment HostEnvironment => HostEnvironmentHelper.Host;
        protected static ILocalEnvironment LocalEnvironment => HostEnvironmentHelper.Host;
        protected IManagedService AutoMessagingService { get; private set; }
        protected static uint LastMessageTickValue => SvcWithAutoMessaging.Service.SvcWithAutoMessagingDaemonOperation.LastMessageTickValue;

        protected static void Yield(int milliseconds = 200) {
            Thread.Sleep(milliseconds);
        }

        protected void ClassInit() {

            TestHostHelper.Initialize(useDaemonDebugMode: false);
            AutoMessagingService = TestHostHelper.AddService(
                SvcWithAutoMessaging.Service.SvcWithAutoMessagingServiceFactory.Create(LocalEnvironment)
            );
            TestHostHelper.StartHost();
            Yield(1000);
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

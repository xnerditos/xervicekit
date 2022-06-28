using System.Collections.Generic;
using System.Threading;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Host;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Client;
using XKit.Lib.Testing;
using XKit.Lib.Testing.TestMessageBrokerSvc;
using SystemTests.ServiceCalls.SvcListensForMessages.Service;

namespace SystemTests.ServiceCalls.Tests {

    public class TestBase {

        private static volatile bool isInited = false;
        private static readonly HostEnvironmentHelper HostEnvironmentHelper = TestHostHelper.HostEnvironmentHelper;
        protected static IDependencyConnector DependencyConnector => HostEnvironmentHelper.DependencyConnector;
        protected static string FabricId => HostEnvironmentHelper.FabricConnector.FabricId;
        protected static IMessageBrokerSvcService TestMessageBrokerService => TestHostHelper.TestMessageBrokerService;
        protected static ISvcListensForMessagesService MessageListeningService { get; private set; }
        protected static IHostEnvironment HostEnvironment => HostEnvironmentHelper.Host;
        protected static ILocalEnvironment LocalEnvironment => HostEnvironmentHelper.Host;

        protected static void ClassInit() {
            
            if (isInited) { return; }
            isInited = true;
            TestHostHelper.Initialize();

            TestHostHelper.AddService(
                SvcSimple.Service.SvcSimpleServiceFactory.Create(LocalEnvironment)
            );
            
            TestHostHelper.AddService(
                SvcWithDependency1.Service.SvcWithDependency1ServiceFactory.Create(LocalEnvironment)
            );

            TestHostHelper.AddService(
                SvcWithDependency2.Service.SvcWithDependency2ServiceFactory.Create(LocalEnvironment)
            );

            TestHostHelper.AddCreateService(
                SvcGeneric.Constants.ServiceDescriptor,
                typeof(SvcGeneric.Service.SvcGenericOperation)
            );

            TestHostHelper.AddService(
                SvcSendsMessages.Service.SvcSendsMessagesServiceFactory.Create(LocalEnvironment)
            );

            MessageListeningService = (ISvcListensForMessagesService) TestHostHelper.AddService(
                SvcListensForMessagesServiceFactory.Create(LocalEnvironment)
            );

            TestHostHelper.StartHost();
        }

        protected static void ClassTeardown() {            
            if (!isInited) { return; }
            isInited = false;
            TestHostHelper.DestroyHost();
        }

        protected static void SetRuntimeConfiguration(
            HostConfigDocument hostConfig = null,
            IDictionary<IReadOnlyDescriptor, object> servicesConfig = null
        ) => TestHostHelper.SetRuntimeConfiguration(hostConfig, servicesConfig);

        protected T CreateClient<T>(
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

using System.Collections.Generic;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Host;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Client;
using XKit.Lib.Testing;
using XKit.Lib.Testing.TestMessageBrokerSvc;
using TestServices.SvcListensForMessages;

namespace SystemTests.ServiceCalls.TestsCommon {

    public abstract class TestBase {

        //private bool isInited = false;
        private IDependencyConnector clientDependencyConnector;
        private TestHostHelper testHelper;
        public TestHostHelper TestHelper => testHelper;
        public HostEnvironmentHelper HostEnvironmentHelper => TestHelper.HostEnvironmentHelper;
        public string FabricId => HostEnvironmentHelper.FabricConnector.FabricId;
        public IMessageBrokerSvcService TestMessageBrokerService => TestHelper.TestMessageBrokerService;
        public ISvcListensForMessagesService MessageListeningService { get; private set; }
        public IHostEnvironment HostEnvironment => HostEnvironmentHelper.Host;
        public ILocalEnvironment LocalEnvironment => HostEnvironmentHelper.Host;

        public void InitTests(TestHostHelper testHelper, IDependencyConnector clientDependencyConnector) {
            
            // if (isInited) { return; }
            // isInited = true;
            this.testHelper = testHelper;
            this.clientDependencyConnector = clientDependencyConnector;

            TestHelper.AddService(
                TestServices.SvcSimple.SvcSimpleServiceFactory.Create(LocalEnvironment)
            );
            
            TestHelper.AddService(
                TestServices.SvcWithDependency1.SvcWithDependency1ServiceFactory.Create(LocalEnvironment)
            );

            TestHelper.AddService(
                TestServices.SvcWithDependency2.SvcWithDependency2ServiceFactory.Create(LocalEnvironment)
            );

            TestHelper.AddCreateService(
                TestServices.SvcGeneric.Constants.ServiceDescriptor,
                typeof(TestServices.SvcGeneric.SvcGenericOperation)
            );

            TestHelper.AddService(
                TestServices.SvcSendsMessages.SvcSendsMessagesServiceFactory.Create(LocalEnvironment)
            );

            MessageListeningService = (ISvcListensForMessagesService) TestHelper.AddService(
                SvcListensForMessagesServiceFactory.Create(LocalEnvironment)
            );

            TestHelper.StartHost();
        }

        public void TeardownTests() {            
            TestHelper.DestroyHost();
            testHelper = null;
        }

        public void SetRuntimeConfiguration(
            HostConfigDocument hostConfig = null,
            IDictionary<IReadOnlyDescriptor, object> servicesConfig = null
        ) => TestHelper.SetRuntimeConfiguration(hostConfig, servicesConfig);

        public T CreateClient<T>(
            IServiceClientFactory<T> factory, 
            ServiceCallTypeParameters callTypeParameters = null
        ) {
            return factory.CreateServiceClient(
                log: TestHelper.Log,
                clientDependencyConnector,
                defaultCallTypeParameters: callTypeParameters ?? ServiceCallTypeParameters.SyncResult()
            );
        }
    }
}

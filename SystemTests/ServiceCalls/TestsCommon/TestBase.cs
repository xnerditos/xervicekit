using System.Collections.Generic;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Host;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Client;
using XKit.Lib.Testing;
using XKit.Lib.Testing.TestMessageBrokerSvc;
using TestServices.SvcListensForMessages;
using System.Linq;

namespace SystemTests.ServiceCalls.TestsCommon {

    public abstract class TestBase {

        private IFabricConnector clientFabricConnector;
        private TestHostHelper testHelper;
        public TestHostHelper TestHelper => testHelper;
        public HostEnvironmentHelper HostEnvironmentHelper => TestHelper.HostEnvironmentHelper;
        public string FabricId => HostEnvironmentHelper.Connector.FabricId;
        public IMessageBrokerSvcService TestMessageBrokerService => TestHelper.TestMessageBrokerService;
        //public ISvcListensForMessagesService MessageListeningService { get; private set; }
        public IXkitHostEnvironment HostEnvironment => HostEnvironmentHelper.Host;

        public void InitTests(TestHostHelper testHelper) {
            
            this.testHelper = testHelper;

            if (TestHelper.Host?.HostRunState != RunStateEnum.Active) { 
                TestHelper.AddService(
                    TestServices.SvcSimple.SvcSimpleServiceFactory.Create(HostEnvironment)
                );
                
                TestHelper.AddService(
                    TestServices.SvcWithDependency1.SvcWithDependency1ServiceFactory.Create(HostEnvironment)
                );

                TestHelper.AddService(
                    TestServices.SvcWithDependency2.SvcWithDependency2ServiceFactory.Create(HostEnvironment)
                );

                TestHelper.AddCreateService(
                    TestServices.SvcGeneric.Constants.ServiceDescriptor,
                    typeof(TestServices.SvcGeneric.SvcGenericOperation)
                );

                TestHelper.AddService(
                    TestServices.SvcSendsMessages.SvcSendsMessagesServiceFactory.Create(HostEnvironment)
                );

                TestHelper.AddService(
                    SvcListensForMessagesServiceFactory.Create(HostEnvironment)
                );

                TestHelper.StartHost();
            }

            clientFabricConnector = TestHelper.HostEnvironment.Connector;
        }

        public ISvcListensForMessagesService GetSvcListensForMessagesService() {
            return HostEnvironment.GetManagedServices(
                collectionName: "SystemTest.ServiceCalls",
                serviceName: "SvcListensForMessages"
            ).First() as ISvcListensForMessagesService;
        }

        public void SetConnectorForTestClients(IFabricConnector clientFabricConnector) {
            this.clientFabricConnector = clientFabricConnector;
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
                clientFabricConnector,
                defaultCallTypeParameters: callTypeParameters ?? ServiceCallTypeParameters.SyncResult()
            );
        }
    }
}

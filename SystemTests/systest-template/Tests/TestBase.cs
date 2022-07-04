using XKit.Lib.Common.Fabric;
using XKit.Lib.Host;
using XKit.Lib.Common.Client;
using XKit.Lib.Testing;
using XKit.Lib.Common.Host;
using System.Collections.Generic;
using XKit.Lib.Common.Registration;

namespace SystemTests._NAMESPACE.Tests {

    public class TestBase {
        private IFabricConnector clientFabricConnector;
        private TestHostHelper testHelper;
        public TestHostHelper TestHelper => testHelper;
        public HostEnvironmentHelper HostEnvironmentHelper => TestHelper.HostEnvironmentHelper;
        public string FabricId => HostEnvironmentHelper.Connector.FabricId;
        public IXkitHostEnvironment HostEnvironment => HostEnvironmentHelper.Host;
        public IXkitEnvironment XkitEnvironment => HostEnvironmentHelper.Host;

        public void InitTests(TestHostHelper testHelper, IFabricConnector clientFabricConnector = null) {
            
            this.testHelper = testHelper;

            // TODO:  Add test services here
            // TestHelper.AddService(
            //     TestServices.SvcSendsMessages.SvcSendsMessagesServiceFactory.Create(HostEnvironment)
            // );

            TestHelper.StartHost();
            this.clientFabricConnector = clientFabricConnector ?? TestHelper.HostEnvironment.Connector;
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

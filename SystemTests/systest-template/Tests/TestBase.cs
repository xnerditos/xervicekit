using XKit.Lib.Common.Fabric;
using XKit.Lib.Host;
using XKit.Lib.Common.Client;
using XKit.Lib.Testing;
using XKit.Lib.Common.Host;
using System.Collections.Generic;
using XKit.Lib.Common.Registration;

namespace SystemTests._NAMESPACE.Tests {

    public class TestBase {
        private IDependencyConnector clientDependencyConnector;
        private TestHostHelper testHelper;
        public TestHostHelper TestHelper => testHelper;
        public HostEnvironmentHelper HostEnvironmentHelper => TestHelper.HostEnvironmentHelper;
        public string FabricId => HostEnvironmentHelper.FabricConnector.FabricId;
        public IHostEnvironment HostEnvironment => HostEnvironmentHelper.Host;
        public ILocalEnvironment LocalEnvironment => HostEnvironmentHelper.Host;

        public void InitTests(TestHostHelper testHelper, IDependencyConnector clientDependencyConnector = null) {
            
            this.testHelper = testHelper;

            // TODO:  Add test services here
            // TestHelper.AddService(
            //     TestServices.SvcSendsMessages.SvcSendsMessagesServiceFactory.Create(LocalEnvironment)
            // );

            TestHelper.StartHost();
            this.clientDependencyConnector = clientDependencyConnector ?? TestHelper.LocalEnvironment.DependencyConnector;
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

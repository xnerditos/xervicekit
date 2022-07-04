using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XKit.Lib.Testing;
using XKit.Lib.Consumer;

namespace SystemTests.ServiceCalls.TestsRemote {

    [TestClass]
    public class FireAndForget : RemoteTestBase {

        private readonly TestsCommon.FireAndForget Tests = new(); 
        private readonly ConsumerHelper ConsumerHelper = new();

        [TestInitialize]
        public void Initialize() { 
            InitializeCommon();
            Tests.InitTests(TestHelper); 
            ConsumerHelper.CreateInitConsumer(
                dependencies: new XKit.Lib.Common.Client.IServiceClientFactory[] {
                    TestServices.SvcSimple.SvcSimpleClientFactory.Factory,
                    TestServices.SvcWithDependency1.SvcWithDependency1ClientFactory.Factory
                },
                initialRegistryAddresses: new[] { "localhost:8090" }
            );
            Tests.SetConnectorForTestClients(ConsumerHelper.Connector);
        }

        [TestMethod]
        public async Task SingleServiceCallSucceeds() => await Tests.Test_SingleServiceCallSucceeds();

        [TestMethod]
        public async Task ChainedServiceCallSucceeds() => await Tests.Test_ChainedServiceCallSucceeds();

        [TestMethod]
        public async Task SingleServiceCallWithErrorIsFine() => await Tests.Test_SingleServiceCallWithErrorIsFine();
    }
}

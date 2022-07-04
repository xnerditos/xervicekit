using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XKit.Lib.Consumer;
using XKit.Lib.Testing;

namespace SystemTests.ServiceCalls.TestsRemote {

    [TestClass]
    public class SyncResult : RemoteTestBase {

        private readonly TestsCommon.SyncResult Tests = new(); 
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

        // For the remote tests, we do not tear down since we use the same
        // one for all tests
        // [TestCleanup]
        // public void Teardown() { 
        //     Tests.TeardownTests(); 
        // }

        [TestMethod]
        public async Task SingleServiceCallHappyPath() => await Tests.Test_SingleServiceCallHappyPath();

        [TestMethod]
        public async Task ChainedServiceCallHappyPath() => await Tests.Test_ChainedServiceCallHappyPath();

        [TestMethod]
        public async Task SingleServiceCallWithError() => await Tests.Test_SingleServiceCallWithError();
    }
}

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XKit.Lib.Consumer;
using XKit.Lib.Testing;

namespace SystemTests.ServiceCalls.TestsRemote {

    [TestClass]
    public class GenericServices : RemoteTestBase {

        private readonly TestsCommon.GenericServices Tests = new(); 
        private readonly ConsumerHelper ConsumerHelper = new();

        [TestInitialize]
        public void Initialize() { 
            InitializeCommon();
            Tests.InitTests(TestHelper); 
            ConsumerHelper.CreateInitConsumer(
                dependencies: new XKit.Lib.Common.Client.IServiceClientFactory[] {
                    TestServices.SvcGeneric.SvcGenericClientFactory.Factory
                },
                initialRegistryAddresses: new[] { "localhost:8090" }
            );
            Tests.SetConnectorForTestClients(ConsumerHelper.Connector);
        }

        [TestMethod]
        public async Task SingleServiceCallHappyPath() => await Tests.Test_SingleServiceCallHappyPath();

        [TestMethod]
        public async Task SingleServiceCallWithError() => await Tests.Test_SingleServiceCallWithError();
    }
}

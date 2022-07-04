using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XKit.Lib.Consumer;
using XKit.Lib.Testing;

namespace SystemTests.ServiceCalls.TestsRemote {

    [TestClass]
    public class Messages : RemoteTestBase {

        private readonly TestsCommon.Messages Tests = new(); 
        private readonly ConsumerHelper ConsumerHelper = new();

        [TestInitialize]
        public void Initialize() { 
            InitializeCommon();
            Tests.InitTests(TestHelper); 
            ConsumerHelper.CreateInitConsumer(
                dependencies: new XKit.Lib.Common.Client.IServiceClientFactory[] {
                    TestServices.SvcSendsMessages.SvcSendsMessagesClientFactory.Factory,
                },
                initialRegistryAddresses: new[] { "localhost:8090" }
            );
            Tests.SetConnectorForTestClients(ConsumerHelper.Connector);
        }

        [TestMethod]
        public async Task ServiceRaisesEvent() => await Tests.Test_ServiceRaisesEvent();

        [TestMethod]
        public async Task ServiceIssuesCommand() => await Tests.Test_ServiceIssuesCommand();

        [TestMethod]
        public async Task SubscriberReceivesEvent() => await Tests.Test_SubscriberReceivesEvent();

        [TestMethod]
        public async Task SubscriberReceivesCommand() => await Tests.Test_SubscriberReceivesCommand();
    }
}

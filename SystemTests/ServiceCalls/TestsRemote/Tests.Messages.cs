using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XKit.Lib.Consumer;
using XKit.Lib.Testing;

namespace SystemTests.ServiceCalls.TestsRemote {

    [TestClass]
    public class Messages {

        private readonly TestsCommon.Messages Tests = new(); 
        private readonly ConsumerHelper ConsumerHelper = new();

        [TestInitialize]
        public void Initialize() { 
            var testHelper = new TestHostHelper();
            RemoteTestHelper.InitAsp();
            testHelper.InitializeRemoteTestHost();
            ConsumerHelper.CreateInitConsumer(
                dependencies: new XKit.Lib.Common.Client.IServiceClientFactory[] {
                    TestServices.SvcSendsMessages.SvcSendsMessagesClientFactory.Factory,
                },
                initialRegistryAddresses: new[] { "localhost:8080" }
            );
            Tests.InitTests(testHelper, ConsumerHelper.DependencyConnector); 
        }

        [TestCleanup]
        public void Teardown() { 
            Tests.TeardownTests(); 
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

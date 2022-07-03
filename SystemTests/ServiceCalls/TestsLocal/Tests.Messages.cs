using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XKit.Lib.Testing;

namespace SystemTests.ServiceCalls.TestsLocal {

    [TestClass]
    public class Messages {

        private readonly TestsCommon.Messages Tests = new(); 

        [TestInitialize]
        public void Initialize() { 
            var testHelper = new TestHostHelper();
            testHelper.InitializeLocalTestHost();
            Tests.InitTests(testHelper, testHelper.Host.DependencyConnector); 
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

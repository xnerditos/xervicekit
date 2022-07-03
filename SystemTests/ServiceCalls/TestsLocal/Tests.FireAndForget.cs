using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XKit.Lib.Testing;

namespace SystemTests.ServiceCalls.TestsLocal {

    [TestClass]
    public class FireAndForget {

        private readonly TestsCommon.FireAndForget Tests = new(); 

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
        public async Task SingleServiceCallSucceeds() => await Tests.Test_SingleServiceCallSucceeds();

        [TestMethod]
        public async Task ChainedServiceCallSucceeds() => await Tests.Test_ChainedServiceCallSucceeds();

        [TestMethod]
        public async Task SingleServiceCallWithErrorIsFine() => await Tests.Test_SingleServiceCallWithErrorIsFine();
    }
}

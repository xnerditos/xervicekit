using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XKit.Lib.Testing;

namespace SystemTests.ServiceCalls.TestsLocal {

    [TestClass]
    public class GenericServices {

        private readonly TestsCommon.GenericServices Tests = new(); 

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
        public async Task SingleServiceCallHappyPath() => await Tests.Test_SingleServiceCallHappyPath();

        [TestMethod]
        public async Task SingleServiceCallWithError() => await Tests.Test_SingleServiceCallWithError();
    }
}

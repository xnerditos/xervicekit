using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XKit.Lib.Testing;
using XKit.Lib.Consumer;

namespace SystemTests.ServiceCalls.TestsRemote {

    [TestClass]
    public class FireAndForget {

        private readonly TestsCommon.FireAndForget Tests = new(); 
        private readonly ConsumerHelper ConsumerHelper = new();

        [TestInitialize]
        public void Initialize() { 
            var testHelper = new TestHostHelper();
            RemoteTestHelper.InitAsp();
            testHelper.InitializeRemoteTestHost();
            ConsumerHelper.CreateInitConsumer(
                dependencies: new XKit.Lib.Common.Client.IServiceClientFactory[] {
                    TestServices.SvcSimple.SvcSimpleClientFactory.Factory,
                    TestServices.SvcWithDependency1.SvcWithDependency1ClientFactory.Factory
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
        public async Task SingleServiceCallSucceeds() => await Tests.Test_SingleServiceCallSucceeds();

        [TestMethod]
        public async Task ChainedServiceCallSucceeds() => await Tests.Test_ChainedServiceCallSucceeds();

        [TestMethod]
        public async Task SingleServiceCallWithErrorIsFine() => await Tests.Test_SingleServiceCallWithErrorIsFine();
    }
}

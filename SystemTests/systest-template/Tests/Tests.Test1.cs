using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XKit.Lib.Testing;

namespace SystemTests._NAMESPACE.Tests {

    [TestClass]
    public class Test1 : TestBase {

        // [ClassInitialize]
        // public static void Initialize(TestContext context) { TestBase.ClassInit(); }

        // [ClassCleanup]
        // public static void Teardown() { TestBase.ClassTeardown(); }

        [TestInitialize]
        public void Initialize() { 
            var testHelper = new TestHostHelper();
            testHelper.InitializeLocalTestHost();
            InitTests(testHelper, testHelper.Host.DependencyConnector); 
        }

        [TestCleanup]
        public void Teardown() { 
            TeardownTests(); 
        }

        [TestMethod]
        public async Task Succeeds() => await TestHelper.RunTestAsync(async () => {
            
            // TODO:  Write Test code

            var client = CreateClient(Svc1.Client.Svc1ClientFactory.Factory);
            var result = await client.GetTestValue(new Svc1.Entities.TestValueRequest {
                TheValue = "test"
            });
            
            result.ImmediateSuccess.Should().BeTrue();
            result.ResponseBody.TheIncomingValue.Should().Be("test");
            result.ResponseBody.RandomValue.Should().NotBeNullOrEmpty();
        });
    }
}

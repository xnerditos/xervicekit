using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XKit.Lib.Testing;

namespace SystemTests.ServiceCalls.Tests {

    [TestClass]
    public class SyncResult : TestBase {

        [TestInitialize]
        public void Initialize() { TestBase.ClassInit(); }

        [TestCleanup]
        public void Teardown() { TestBase.ClassTeardown(); }

        // [ClassInitialize]
        // public static void ClassInitialize() { TestBase.ClassInit(); }

        // [ClassCleanup]
        // public static void ClassTeardown() { TestBase.ClassTeardown(); }

        [TestMethod]
        public async Task SingleServiceCallHappyPath() => await TestHostHelper.RunTestAsync(async () => {
            
            string testValue = Guid.NewGuid().ToString();
            var client = CreateClient(SvcSimple.Client.SvcSimpleClientFactory.Factory);
            var result = await client.GetTestValueNoDependencies(new SvcSimple.Entities.TestValueRequest {
                TheValue = testValue
            });
            
            result.ImmediateSuccess.Should().BeTrue();
            result.ResponseBody.TheIncomingValue.Should().Be(testValue);
            result.ResponseBody.RandomValue.Should().NotBeNullOrEmpty();
        });

        [TestMethod]
        public async Task ChainedServiceCallHappyPath() => await TestHostHelper.RunTestAsync(async () => {
            
            string testValue = Guid.NewGuid().ToString();
            var client = CreateClient(SvcWithDependency1.Client.SvcWithDependency1ClientFactory.Factory);
            var result = await client.GetTestValueWithDependency2Levels(new SvcWithDependency1.Entities.TestValueRequest {
                TheValue = testValue
            });
            
            result.ImmediateSuccess.Should().BeTrue();
            result.ResponseBody.TheIncomingValue.Should().Be(testValue);
            result.ResponseBody.RandomValue.Should().NotBeNullOrEmpty();
        });

        [TestMethod]
        public async Task SingleServiceCallWithError() => await TestHostHelper.RunTestAsync(async () => {

            var client = CreateClient(SvcSimple.Client.SvcSimpleClientFactory.Factory);
            var result = await client.Fails();
            
            result.ImmediateSuccess.Should().BeFalse();
            result.ResponseBody.Should().BeNull();
        });
    }
}

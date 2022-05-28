using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XKit.Lib.Testing;

namespace SystemTests.ServiceCalls.Tests {

    [TestClass]
    public class GenericServices : TestBase {

        [TestInitialize]
        public void Initialize() { TestBase.ClassInit(); }

        [TestCleanup]
        public void Teardown() { TestBase.ClassTeardown(); }

        [TestMethod]
        public async Task SingleServiceCallHappyPath() => await TestHostHelper.RunTestAsync(async () => {
            
            string testValue = Guid.NewGuid().ToString();
            var client = CreateClient(SvcGeneric.Client.SvcGenericClientFactory.Factory);
            var result = await client.GetTestValueNoDependencies(new SvcGeneric.Entities.TestValueRequest {
                TheValue = testValue
            });
            
            result.ImmediateSuccess.Should().BeTrue();
            result.ResponseBody.TheIncomingValue.Should().Be(testValue);
            result.ResponseBody.RandomValue.Should().NotBeNullOrEmpty();
        });

        [TestMethod]
        public async Task SingleServiceCallWithError() => await TestHostHelper.RunTestAsync(async () => {

            var client = CreateClient(SvcGeneric.Client.SvcGenericClientFactory.Factory);
            var result = await client.Fails();
            
            result.ImmediateSuccess.Should().BeFalse();
            result.ResponseBody.Should().BeNull();
        });
    }
}

using System;
using System.Threading.Tasks;
using FluentAssertions;
using XKit.Lib.Common.Fabric;

namespace SystemTests.ServiceCalls.TestsCommon {

    public class SyncResult : TestBase {

        public async Task Test_SingleServiceCallHappyPath() => await TestHelper.RunTestAsync(async () => {
            
            string testValue = Guid.NewGuid().ToString();
            var client = CreateClient(TestServices.SvcSimple.SvcSimpleClientFactory.Factory);
            var result = await client.GetTestValueNoDependencies(new TestServices.SvcSimple.TestValueRequest {
                TheValue = testValue
            });
            
            result.ImmediateSuccess.Should().BeTrue();
            result.ResponseBody.TheIncomingValue.Should().Be(testValue);
            result.ResponseBody.RandomValue.Should().NotBeNullOrEmpty();
        });

        public async Task Test_ChainedServiceCallHappyPath() => await TestHelper.RunTestAsync(async () => {
            
            string testValue = Guid.NewGuid().ToString();
            var client = CreateClient(TestServices.SvcWithDependency1.SvcWithDependency1ClientFactory.Factory);
            var result = await client.GetTestValueWithDependency2Levels(new TestServices.SvcWithDependency1.TestValueRequest {
                TheValue = testValue
            });
            
            result.ImmediateSuccess.Should().BeTrue();
            result.ResponseBody.TheIncomingValue.Should().Be(testValue);
            result.ResponseBody.RandomValue.Should().NotBeNullOrEmpty();
        });

        public async Task Test_SingleServiceCallWithError() => await TestHelper.RunTestAsync(async () => {

            var client = CreateClient(TestServices.SvcSimple.SvcSimpleClientFactory.Factory);
            var result = await client.Fails();
            
            result.ImmediateSuccess.Should().BeFalse();
            result.ResponseBody.Should().BeNull();
        });
    }
}

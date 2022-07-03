using System;
using System.Threading.Tasks;
using FluentAssertions;
using XKit.Lib.Testing;

namespace SystemTests.ServiceCalls.TestsCommon {

    public class GenericServices : TestBase {

        public async Task Test_SingleServiceCallHappyPath() => await TestHelper.RunTestAsync(async () => {
            
            string testValue = Guid.NewGuid().ToString();
            var client = CreateClient(TestServices.SvcGeneric.SvcGenericClientFactory.Factory);
            var result = await client.GetTestValueNoDependencies(new TestServices.SvcGeneric.TestValueRequest {
                TheValue = testValue
            });
            
            result.ImmediateSuccess.Should().BeTrue();
            result.ResponseBody.TheIncomingValue.Should().Be(testValue);
            result.ResponseBody.RandomValue.Should().NotBeNullOrEmpty();
        });

        public async Task Test_SingleServiceCallWithError() => await TestHelper.RunTestAsync(async () => {

            var client = CreateClient(TestServices.SvcGeneric.SvcGenericClientFactory.Factory);
            var result = await client.Fails();
            
            result.ImmediateSuccess.Should().BeFalse();
            result.ResponseBody.Should().BeNull();
        });
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using FluentAssertions;
using SystemTests.ServiceCalls.Environment;
using XKit.Lib.Common.Log;
using XKit.Lib.Testing;

namespace SystemTests.ServiceCalls.TestsCommon {

    public class FireAndForget : TestBase {

        public async Task Test_SingleServiceCallSucceeds() => await TestHelper.RunTestAsync(async () => {
            string testValue = Guid.NewGuid().ToString();
            
            var client = CreateClient(
                TestServices.SvcSimple.SvcSimpleClientFactory.Factory,
                ServiceCallTypeParameters.FireAndForget()
            );

            ValueHelper.ClearJsonTestData();

            var result = await client.ChangeStaticValue(new TestServices.SvcSimple.TestValueRequest {
                TheValue = testValue
            });
            
            result.ServiceCallStatus.Should().Be(ServiceCallStatusEnum.Completed);
            result.OperationStatus.Should().Be(LogResultStatusEnum.Pending);
            Thread.Sleep(1000);

            var testData = 
                ValueHelper.GetJsonTestData<string>();
            
            testData.Should().Be(testValue);
        });

        public async Task Test_ChainedServiceCallSucceeds() => await TestHelper.RunTestAsync(async () => {
            
            string testValue = Guid.NewGuid().ToString();
            var client = CreateClient(
                TestServices.SvcWithDependency1.SvcWithDependency1ClientFactory.Factory,
                ServiceCallTypeParameters.FireAndForget()
            );

            ValueHelper.ClearJsonTestData();

            var result = await client.ChangeStaticValueWithDependency2Levels(new TestServices.SvcWithDependency1.TestValueRequest {
                TheValue = testValue
            });

            result.ServiceCallStatus.Should().Be(ServiceCallStatusEnum.Completed);
            result.OperationStatus.Should().Be(LogResultStatusEnum.Pending);
            Thread.Sleep(1000);
            var testData = 
                ValueHelper.GetJsonTestData<string>();
            
            testData.Should().Be(testValue);
        });

        public async Task Test_SingleServiceCallWithErrorIsFine() => await TestHelper.RunTestAsync(async () => {
            
            var client = CreateClient(
                TestServices.SvcSimple.SvcSimpleClientFactory.Factory,
                ServiceCallTypeParameters.FireAndForget()
            );
            var result = await client.Fails();
            
            Thread.Sleep(1000);
            result.ServiceCallStatus.Should().Be(ServiceCallStatusEnum.Completed);
            result.OperationStatus.Should().Be(LogResultStatusEnum.Pending);
        });
    }
}

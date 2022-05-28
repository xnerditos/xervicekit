using System;
using System.Threading;
using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SystemTests.ServiceCalls.Environment;
using XKit.Lib.Common.Log;
using XKit.Lib.Testing;

namespace SystemTests.ServiceCalls.Tests {

    [TestClass]
    public class FireAndForget : TestBase {

        [TestInitialize]
        public void Initialize() { TestBase.ClassInit(); }

        [TestCleanup]
        public void Teardown() { TestBase.ClassTeardown(); }

        [TestMethod]
        public async Task SingleServiceCallSucceeds() => await TestHostHelper.RunTestAsync(async () => {
            string testValue = Guid.NewGuid().ToString();
            
            var client = CreateClient(
                SvcSimple.Client.SvcSimpleClientFactory.Factory,
                ServiceCallTypeParameters.FireAndForget()
            );

            ValueHelper.ClearJsonTestData();

            var result = await client.ChangeStaticValue(new SvcSimple.Entities.TestValueRequest {
                TheValue = testValue
            });
            
            result.ServiceCallStatus.Should().Be(ServiceCallStatusEnum.Completed);
            result.OperationStatus.Should().Be(LogResultStatusEnum.Pending);
            Thread.Sleep(1000);

            var testData = 
                ValueHelper.GetJsonTestData<string>();
            
            testData.Should().Be(testValue);
        });

        [TestMethod]
        public async Task ChainedServiceCallSucceeds() => await TestHostHelper.RunTestAsync(async () => {
            
            string testValue = Guid.NewGuid().ToString();
            var client = CreateClient(
                SvcWithDependency1.Client.SvcWithDependency1ClientFactory.Factory,
                ServiceCallTypeParameters.FireAndForget()
            );

            ValueHelper.ClearJsonTestData();

            var result = await client.ChangeStaticValueWithDependency2Levels(new SvcWithDependency1.Entities.TestValueRequest {
                TheValue = testValue
            });

            result.ServiceCallStatus.Should().Be(ServiceCallStatusEnum.Completed);
            result.OperationStatus.Should().Be(LogResultStatusEnum.Pending);
            Thread.Sleep(1000);
            var testData = 
                ValueHelper.GetJsonTestData<string>();
            
            testData.Should().Be(testValue);
        });

        [TestMethod]
        public async Task SingleServiceCallWithErrorIsFine() => await TestHostHelper.RunTestAsync(async () => {
            
            var client = CreateClient(
                SvcSimple.Client.SvcSimpleClientFactory.Factory,
                ServiceCallTypeParameters.FireAndForget()
            );
            var result = await client.Fails();
            
            Thread.Sleep(1000);
            result.ServiceCallStatus.Should().Be(ServiceCallStatusEnum.Completed);
            result.OperationStatus.Should().Be(LogResultStatusEnum.Pending);
        });
    }
}

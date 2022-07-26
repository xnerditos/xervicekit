using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;
using Samples.SampleService.V1.ServiceApiEntities;
using Samples.SampleService.V1;
using XKit.Lib.Common.Utility.Extensions;

namespace Tests.Services.SampleService {

    [TestClass]
    public class SampleCall : TestBase {

        [ClassInitialize]
        public static void ClassInitialize(TestContext _) {
            ClassInit();

            // TODO:  Any special config goes here
            SetRuntimeConfiguration(
                servicesConfig: new Dictionary<IReadOnlyDescriptor, object>()
            );
        }

        [ClassCleanup]
        public static void ClassCleanup() {
            ClassTeardown();
        }

        [TestMethod]
        public Task FailsWithBadRequest() => TestHelper.RunTestAsync(async () => {

            // ---- Arrange
            var client = CreateClient();
            var request = new SampleRequest {
                SomeCollection = null,
                SomeValue = 0
            };

            // ---- Act
            var result = await client.SampleCall(request);

            // ---- Assert
            result.HasError.Should().BeTrue();
            result.OperationStatus.Should().Be(LogResultStatusEnum.NoAction_BadRequest);
        });

        [TestMethod]
        public Task CreatesCorrectResponseHappyPath() => TestHelper.RunTestAsync(async () => {

            // ---- Arrange
            var client = CreateClient();
            var request = new SampleRequest {
                SomeCollection = new[] { "hello", "world" },
                SomeValue = 1
            };

            SampleEventMessage message = null;
            MessageBroker.InterceptMessage<SampleServiceEvents>(
                nameof(SampleServiceEvents.SampleEvent),
                (sentMessage, results) => {
                    message = sentMessage.JsonPayload.FromJson<SampleEventMessage>();
                }
            );

            // ---- Act
            var result = await client.SampleCall(request);

            // ---- Assert
            result.ImmediateSuccess.Should().BeTrue();
            result.ResponseBody.Should().NotBeNull();
            result.ResponseBody.SomeValue.Should().Be(1);
            result.ResponseBody.AFutureDate.Should().BeAfter(DateTime.Today);
            result.ResponseBody.RandomValue.Should().NotBeNullOrEmpty();
            result.ResponseBody.SomeCollection.Length.Should().Be(3);
            message.Should().NotBeNull();
            message.Should().BeEquivalentTo(new SampleEventMessage { SomeEventData = "Test" });
        });
    }
}

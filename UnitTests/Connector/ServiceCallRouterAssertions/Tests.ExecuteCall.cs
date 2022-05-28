using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Registration;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UnitTests.Connector.Entities;
using XKit.Lib.Common.Log;
using System.Linq;

namespace UnitTests.Connector.ServiceCallRouterAssertions {
    
    public partial class Tests {

        [TestMethod]
        public async Task ExecuteCall_CorrectlyCallsInstance() {

            ClearMocks();
            var request = CreateRequest();
            var responderFabricId = CreateRandomString();
            var instResult = CreateServiceResult(responderFabricId: responderFabricId);
            instResult.Service = Constants.ServiceDescriptor;
            var client1 = SetupInstanceClient(
                modelRequest: request,
                response: instResult,
                hostFabricId: responderFabricId
            );
            var target = (IServiceCallRouter)CreateTarget();
            var result = (await target.ExecuteCall(request, null, null)).First();

            result.Should().BeEquivalentTo(new ServiceCallResult<TestResponse> {
                OperationName = Constants.OperationName,
                ResponderFabricId = responderFabricId,
                ServiceCallStatus = ServiceCallStatusEnum.Completed,
                ResponseBody = new TestResponse { Something = instResult.ResponseBody.Something },
                OperationStatus = LogResultStatusEnum.Success,
                Service = Constants.ServiceDescriptor.Clone(),
                CorrelationId = request.CorrelationId,
                RequestorFabricId = request.RequestorFabricId,
                RequestorInstanceId = request.RequestorInstanceId,
            }, c => c.ExcludingMissingMembers());
            
            client1.Verify_ExecuteOperation(request, Times.Once);
        }

        // NOTE:  Algorithm for determining preference is non-deterministic
        // [TestMethod]
        // public async Task ExecuteCall_PrefersHealthiestInstance() {

        //     var request = CreateRequest();

        //     var client1 = SetupInstanceClient(
        //         modelRequest: request,
        //         health: HealthEnum.Healthy
        //     );
        //     var client2 = SetupInstanceClient(health: HealthEnum.Moderate);
        //     var client3 = SetupInstanceClient(health: HealthEnum.UnhealthyRecovering);

        //     var target = (IServiceCallRouter)CreateTarget();

        //     var response = await target.Execute<TestRequest, TestResponse>(request, null);

        //     client1.Verify_ExecuteOperation(request, Times.Once);
        //     client2.Verify_ExecuteOperation(request, Times.Never);
        //     client3.Verify_ExecuteOperation(request, Times.Never);
        // }

        [TestMethod]
        public async Task ExecuteCall_PrefersMostAvailableInstance() {

            ClearMocks();
            var request = CreateRequest();
            var client1 = SetupInstanceClient(
                modelRequest: request,
                response: CreateServiceResult(),
                health: HealthEnum.Healthy,
                availability: AvailabilityEnum.Serving9
            );
            var client2 = SetupInstanceClient(health: HealthEnum.Healthy, availability: AvailabilityEnum.Serving5);
            var client3 = SetupInstanceClient(health: HealthEnum.Healthy, availability: AvailabilityEnum.Serving5);

            var target = (IServiceCallRouter)CreateTarget();

            var result = await target.ExecuteCall(request, null, null);

            client1.Verify_ExecuteOperation(request, Times.Once);
            client2.Verify_ExecuteOperation(request, Times.Never);
            client3.Verify_ExecuteOperation(request, Times.Never);
        }

        [TestMethod]
        public async Task ExecuteCall_PrefersLocalInstance() {

            ClearMocks();
            var fabricId = CreateRandomString();
            var request = CreateRequest(requestorFabricId: fabricId);
            var client1 = SetupInstanceClient(
                modelRequest: request,
                response: CreateServiceResult(),
                hostFabricId: fabricId
            );
            var client2 = SetupInstanceClient();
            var client3 = SetupInstanceClient();

            var target = (IServiceCallRouter)CreateTarget();

            var response = await target.ExecuteCall(request, null, null);

            client1.Verify_ExecuteOperation(request, Times.Once);
            client2.Verify_ExecuteOperation(request, Times.Never);
            client3.Verify_ExecuteOperation(request, Times.Never);
        }

        [TestMethod]
        public async Task ExecuteCall_CallsSecondInstanceIfFirstFails() {

            ClearMocks();
            var request = CreateRequest();

            var responseValue = CreateRandomString();
            var client1 = SetupInstanceClient(
                modelRequest: request,
                response: CreateServiceResult(
                    ServiceCallStatusEnum.NoConnection, 
                    operationStatus: LogResultStatusEnum.NoAction_Timeout
                ),
                health: HealthEnum.Healthy,
                availability: AvailabilityEnum.Serving9 // this one should be selected first
            );
            var client2 = SetupInstanceClient(
                modelRequest: request,
                response: CreateServiceResult(somethingValue: responseValue),
                health: HealthEnum.Healthy,
                availability: AvailabilityEnum.Serving6 // this one should be selected second
            );
            var client3 = SetupInstanceClient(
                modelRequest: request,
                response: CreateServiceResult(somethingValue: responseValue),
                health: HealthEnum.UnhealthyRecovering,
                availability: AvailabilityEnum.Serving5 // this one should be last in the list but never used
            );

            var target = (IServiceCallRouter)CreateTarget();

            var results = await target.ExecuteCall(request, null, null);
            results.Count.Should().Be(2);
            results[0].Completed.Should().BeFalse();
            results[1].Completed.Should().BeTrue();
            results[1].GetBody<TestResponse>().Something.Should().Be(responseValue);            
            client1.Verify_ExecuteOperation(request, Times.Once);
            client2.Verify_ExecuteOperation(request, Times.Once);
            client3.Verify_ExecuteOperation(request, Times.Never);
        }
    
        [TestMethod]
        public async Task ExecuteCall_CallsSpecificInstance() {

            ClearMocks();
            var initialRequest = CreateRequest();
            var secondaryRequest = initialRequest.Clone();

            var client1 = SetupInstanceClient(
                //modelRequest: initialRequest,
                response: CreateServiceResult(),
                availability: AvailabilityEnum.Serving9
            );
            var client2 = SetupInstanceClient(
                //modelRequest: secondaryRequest,
                response: CreateServiceResult(),
                availability: AvailabilityEnum.Serving8
            );
            string targetHostId = nameof(targetHostId);
            var client3 = SetupInstanceClient(
                //modelRequest: secondaryRequest,
                response: CreateServiceResult(),
                availability: AvailabilityEnum.Serving8,
                hostFabricId: targetHostId
            );

            var target = (IServiceCallRouter)CreateTarget();

            var result = await target.ExecuteCall(
                initialRequest, 
                null,
                new ServiceCallPolicy {
                    CallPattern = ServiceCallPatternEnum.SpecificHost
                },
                targetHostId
            );

            client1.Verify_ExecuteOperation(matchRequest: null, Times.Never);
            client2.Verify_ExecuteOperation(matchRequest: null, Times.Never);
            client3.Verify_ExecuteOperation(matchRequest: null, Times.Once);
        }
    }
}

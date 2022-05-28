using System;
using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Registration;
using FluentAssertions;
using Moq;

namespace UnitTests.MockWrapper {

    public class InstanceClientMockWrapper : MockWrapperBase<IInstanceClient> {

        // =====================================================================
        // Setup
        // =====================================================================

        public void Setup_All<TRequestBody, TResponseBody>(
            ServiceCallRequest<TRequestBody> modelRequest,
            ServiceCallResult<TResponseBody> response,
            IReadOnlyDescriptor serviceDescriptor,
            HealthEnum health = HealthEnum.Healthy,
            AvailabilityEnum availability = AvailabilityEnum.Serving5,
            string hostFabricId = null
        ) where TRequestBody : class where TResponseBody : class {

            Setup_ExecuteOperation(
                (req) => modelRequest == null ? true : req.OperationName == modelRequest.OperationName,
                response,
                modelRequest
            );

            Setup_Instance(serviceDescriptor, health, availability, hostFabricId);
        }

        public void Setup_Instance(
            IReadOnlyDescriptor serviceDescriptor,
            HealthEnum health = HealthEnum.Healthy,
            AvailabilityEnum availability = AvailabilityEnum.Serving5,
            string hostFabricId = null
        ) {
            
            var svcInstanceInfo = new ServiceInstance {
                Descriptor = serviceDescriptor.Clone(),
                HostFabricId = hostFabricId ?? Guid.NewGuid().ToString(),
                Status = new ServiceInstanceStatus {
                    RunState = RunStateEnum.Active,
                    Health = health,
                    Availability = availability
                }
            };
            Mock.SetupGet(x => x.Instance)
                .Returns(svcInstanceInfo);
        }

        public void Setup_ExecuteOperation(
            Func<ServiceCallRequest, bool> matchRequest, 
            ServiceCallResult result,
            ServiceCallRequest validateModelRequest = null
        ) {

            Mock.Setup(
                x => x.ExecuteCall(
                    It.Is<ServiceCallRequest>(
                        sr => matchRequest == null ? true : matchRequest(sr))
                )
            ).Returns((ServiceCallRequest req) => {
                if (validateModelRequest != null) {
                    req.Should().BeEquivalentTo(validateModelRequest);
                }
                return Task.FromResult(result);
            });
        }

        // =====================================================================
        // Verify
        // =====================================================================

        public void Verify_ExecuteOperation(
            ServiceCallRequest request, 
            Func<Times> times
        ) {
            Mock.Verify(x => x.ExecuteCall(
                It.Is<ServiceCallRequest>(x => 
                    x.CorrelationId == request.CorrelationId &&
                    x.OperationName == request.OperationName &&
                    x.RequestorFabricId == request.RequestorFabricId &&
                    x.RequestorInstanceId == request.RequestorInstanceId
                )), 
                times
            );            
        }

        public void Verify_ExecuteOperation(
            Func<ServiceCallRequest, bool> matchRequest, 
            Func<Times> times
        ) {
            Mock.Verify(
                x => x.ExecuteCall(
                    It.Is<ServiceCallRequest>(
                        req => matchRequest == null ? true : matchRequest(req)
                    )
                ), 
                times
            );            
        }
    }
}
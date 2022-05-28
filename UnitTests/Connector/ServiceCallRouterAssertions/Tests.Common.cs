using System;
using System.Collections.Generic;
using System.Linq;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Registration;
using XKit.Lib.Connector.Dependency;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Connector.Entities;
using UnitTests.MockWrapper;
using XKit.Lib.Common.Log;

namespace UnitTests.Connector.ServiceCallRouterAssertions {
    
    [TestClass]
    public partial class Tests : TestBase {

        public readonly List<InstanceClientMockWrapper> InstanceClients = new();
        
        // =====================================================================
        // create
        // =====================================================================
        private ServiceCallRequest<TestRequest> CreateRequest(
            ServiceCallTypeParameters callTypeParameters = null,
            string requestorFabricId = null
        ) => ServiceCallRequest<TestRequest>.Create(
            operationName: Constants.OperationName,
            requestBody: new TestRequest { Something = CreateRandomString() },
            requestorFabricId: requestorFabricId ?? CreateRandomString(),
            requestorInstanceId: CreateRandomString(),
            correlationId: null,
            callTypeParameters: callTypeParameters ?? ServiceCallTypeParameters.SyncResult()
        );

        private void ClearMocks() => InstanceClients.Clear();
        
        private ServiceCallResult<TestResponse> CreateServiceResult(
            ServiceCallStatusEnum callStatus = ServiceCallStatusEnum.Completed,
            LogResultStatusEnum operationStatus = LogResultStatusEnum.Success,
            string somethingValue = null,
            string responderFabricId = null
        ) => new() {
                    OperationName = Constants.OperationName,
                    ResponseBody = new TestResponse { Something = somethingValue ?? Constants.RandomString },
                    ServiceCallStatus = callStatus,
                    OperationStatus = operationStatus,
                    ResponderFabricId = responderFabricId ?? CreateRandomString()
                };

        private ServiceCallRouter CreateTarget() 
            => new(
                new ServiceRegistration {
                    Descriptor = Constants.ServiceDescriptor,
                    CallPolicy = new ServiceCallPolicy {
                        CallPattern = ServiceCallPatternEnum.FirstChance
                    }
                },
                DateTime.MaxValue,
                InstanceClients.Select(c => c.Object)
            );

        // =====================================================================
        // setup
        // =====================================================================
        private InstanceClientMockWrapper SetupInstanceClient(
            ServiceCallRequest<TestRequest> modelRequest = null,
            ServiceCallResult<TestResponse> response = null,
            HealthEnum health = HealthEnum.Healthy,
            AvailabilityEnum availability = AvailabilityEnum.Serving5,
            string hostFabricId = null
        ) {
            var wrapper = Mocks.CreateWrapper<InstanceClientMockWrapper>();
            response ??= CreateServiceResult();
            wrapper.Setup_All(
                modelRequest,
                response,
                Constants.ServiceDescriptor,
                health,
                availability,
                hostFabricId
            );
            InstanceClients.Add(wrapper);
            return wrapper;
        }
    }
}

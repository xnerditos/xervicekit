using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Utility;
using FluentAssertions;
using Moq;

namespace UnitTests.MockWrapper {

    public class ServiceCallRouterMockWrapper : MockWrapperBase<IServiceCallRouter> {

        private IReadOnlyDescriptor serviceDescriptor;

        // =====================================================================
        // Init
        // =====================================================================
        public void SetTargetService(
            IReadOnlyDescriptor serviceDescriptor,
            bool setupRegistrationKeyAlso = false
        ) {
            this.serviceDescriptor = serviceDescriptor;
            if (setupRegistrationKeyAlso) {
                Setup_RegistrationKey();
            }
        }

        // =====================================================================
        // Setup
        // =====================================================================

        public void Setup_RegistrationKey() {
            Mock.SetupGet(x => x.ServiceRegistrationKey)
                .Returns(Identifiers.GetServiceFullRegistrationKey(this.serviceDescriptor));
        }

        public void Setup_ExecuteCall<TRequestBody, TResponseBody>(
            Func<ServiceCallRequest, bool> checkRequest, 
            ServiceCallResult result,
            TRequestBody expectedRequestBody = null
        ) where TResponseBody : class where TRequestBody : class {
            
            Mock.Setup(
                x => x.ExecuteCall(
                    It.Is<ServiceCallRequest>(sr => checkRequest == null || checkRequest(sr)), 
                    It.IsAny<ILogSession>(),
                    It.IsAny<ServiceCallPolicy>(),
                    It.IsAny<string>()
                )
            ).Returns((ServiceCallRequest<TRequestBody> request, ILogSession log, ServiceCallPolicy policy, string targetHostId) => {
                if (expectedRequestBody != null) {
                    request.RequestBody.Should().BeEquivalentTo(expectedRequestBody);
                }
                return Task.FromResult((IReadOnlyList<ServiceCallResult>) new[] { result });
            });
        }

        public void Setup_ExecuteCall<TRequestBody>(
            Func<ServiceCallRequest, bool> checkRequest, 
            ServiceCallResult result,
            TRequestBody expectedRequestBody = null
        ) where TRequestBody : class {
            
            Mock.Setup(
                x => x.ExecuteCall(
                    It.Is<ServiceCallRequest<TRequestBody>>(sr => checkRequest == null || checkRequest(sr)), 
                    It.IsAny<ILogSession>(),
                    It.IsAny<ServiceCallPolicy>(),
                    It.IsAny<string>()
                )
            ).Returns((ServiceCallRequest<TRequestBody> request) => {
                if (expectedRequestBody != null) {
                    request.RequestBody.Should().BeEquivalentTo(expectedRequestBody);
                }
                return Task.FromResult((IReadOnlyList<ServiceCallResult>) new[] { result });
            });
        }
    }
}

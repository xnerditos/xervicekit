using System;
using System.Collections.Generic;
using System.Linq;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Registration;
using XKit.Lib.Connector.Dependency;
using FluentAssertions;
using Moq;
using XKit.Lib.Common.Log;

namespace UnitTests.MockWrapper {

    public class ServiceCallFactoryMockWrapper : MockWrapperBase<IServiceCallRouterFactory> {

        // =====================================================================
        // Setup
        // =====================================================================

        // public ServiceCallRouterMockWrapper Setup_Create(
        //     IReadOnlyDescriptor expectedDescriptor,
        //     IEnumerable<InstanceClientMockWrapper> mockClients
        // ) => Setup_Create(
        //     expectedDescriptor, 
        //     mockClients, 
        //     MockWrappersManager.CreateWrapper<ServiceCallRouterMockWrapper>()
        // );

        public ServiceCallRouterMockWrapper Setup_Create(
            IReadOnlyDescriptor expectedDescriptor,
            IEnumerable<InstanceClientMockWrapper> mockClients,
            ServiceCallRouterMockWrapper serviceCallRouter
        ) {
            serviceCallRouter?.SetTargetService(expectedDescriptor);
            Mock.Setup(
                x => x.Create(
                    It.Is<IReadOnlyServiceRegistration>(
                        p => p.Descriptor.IsSameService(expectedDescriptor)
                    ),
                    It.IsAny<DateTime>(),
                    It.IsAny<IEnumerable<IInstanceClient>>()
                )
            ).Callback((IReadOnlyServiceRegistration r, DateTime d, IEnumerable<IInstanceClient> clients) => {
                if (mockClients != null) {
                    clients.Should().BeEquivalentTo(mockClients.Select(c => c.Object));
                }
            }).Returns(serviceCallRouter?.Object);
            
            return serviceCallRouter;
        }

        // =====================================================================
        // Verify
        // =====================================================================

        public void Verify_Create(
            IReadOnlyDescriptor expectedDescriptor,
            Func<Times> times = null
        ) {
            times ??= Times.Once;
            Mock.Verify(
                x => x.Create(
                    It.Is<IReadOnlyServiceRegistration>(
                        p => p.Descriptor.IsSameService(expectedDescriptor)
                    ),
                    It.IsAny<DateTime>(),
                    It.IsAny<IEnumerable<IInstanceClient>>()
                ),
                times
            );
        }
    }
}

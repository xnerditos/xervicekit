using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Client;
using Moq;

namespace UnitTests.MockWrapper {

    public class InstanceClientFactoryMockWrapper : MockWrapperBase<IInstanceClientFactory> {

        // =====================================================================
        // Setup
        // =====================================================================

        public void Setup_TryCreateClient(
            string address,
            IReadOnlyDescriptor serviceDescriptor,
            InstanceClientMockWrapper instanceClient
        ) {
            Mock.Setup(
                x => x.TryCreateClient(It.Is<IReadOnlyServiceInstance>(
                    p => p.HostAddress == address && p.Descriptor.IsSameService(serviceDescriptor)
                ))
            ).Returns(instanceClient?.Object);
        }

        public void Setup_InitializeFactory() {
            // NOTE:  We can't easily know the fabric id at the time of setup
            Mock.Setup(x => x.InitializeFactory(It.IsAny<ILocalEnvironment>()));
        }
    }
}

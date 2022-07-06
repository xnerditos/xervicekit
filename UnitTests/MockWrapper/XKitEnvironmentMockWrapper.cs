using System;
using System.Collections.Generic;
using UnitTests.Connector.FabricConnectorAssertions;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Utility;

namespace UnitTests.MockWrapper {

    public class XKitEnvironmentMockWrapper : MockWrapperBase<IXKitEnvironment> {
        
        // =====================================================================
        // overrides
        // =====================================================================

        public override void VerifyMock() {
            // Do not verify    
        }

        // =====================================================================
        // Setup
        // =====================================================================

        public void SetupAll(
            string fabricId = null,
            IEnumerable<IReadOnlyDescriptor> dependencies = null
        ) {
            //Setup_HostEnvironment(hostEnvironmentMockWrapper);
            Setup_FabricId(fabricId ?? Identifiers.GenerateIdentifier());
            Setup_Dependencies(dependencies ?? new[] { TestConstants.Dependency1 });
        }

        // public void Setup_HostEnvironment(HostEnvironmentMockWrapper hostEnvironmentMockWrapper) {
        //     HostEnvironmentMock = hostEnvironmentMockWrapper ?? new HostEnvironmentMockWrapper();
        //     Mock.Setup(x => x.HostEnvironment).Returns(HostEnvironmentMock.Object);
        // }

        public void Setup_FabricId(string hostId) {
            Mock.SetupGet(x => x.FabricId).Returns(hostId);
        }

        public void Setup_Dependencies(IEnumerable<IReadOnlyDescriptor> dependencies = null) {
            dependencies ??= Array.Empty<IReadOnlyDescriptor>();
            Mock.Setup(x => x.GetDependencies()).Returns(dependencies);
        }
    }
}

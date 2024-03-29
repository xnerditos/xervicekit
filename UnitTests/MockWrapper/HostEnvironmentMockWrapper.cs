using System.Collections.Generic;
using System.Linq;
using UnitTests.Connector.FabricConnectorAssertions;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Utility;

namespace UnitTests.MockWrapper {

    public class HostEnvironmentMockWrapper : MockWrapperBase<IXKitHostEnvironment>
    {
        public HostEnvironmentMockWrapper() {
            Setup_HostRunState();
        }

        // =====================================================================
        // overrides
        // =====================================================================

        public override void VerifyMock() {
            // Do not verify the FabricEnvironmentMockWrapper    
        }

        // =====================================================================
        // Setup
        // =====================================================================

        public void SetupAll(
            string hostAddress,
            HealthEnum health = HealthEnum.Healthy,
            string fabricId = null,
            IEnumerable<IReadOnlyServiceRegistration> hostedServices = null,
            IEnumerable<ServiceInstanceStatus> serviceStatuses = null,
            IEnumerable<IReadOnlyDescriptor> dependencies = null
        ) {
            hostedServices ??= System.Array.Empty<ServiceRegistration>();
            Setup_Address(hostAddress);
            Setup_GetCapabilities();
            Setup_GetHealth(health);
            Setup_GetHostedServiceStatuses(serviceStatuses);
            Setup_HasHostedServices(hostedServices.Any());
            Setup_GetHostedServices(hostedServices);            
            Setup_FabricId(fabricId ?? Identifiers.GenerateIdentifier());
            Setup_Dependencies(dependencies ?? new[] { TestConstants.Dependency1 });
        }

        public void Setup_GetHealth(
            HealthEnum health = HealthEnum.Healthy
        ) {
            Mock.Setup(x => x.GetHealth())
                .Returns(health);
        }

        public void Setup_GetHostedServiceStatuses(
            IEnumerable<ServiceInstanceStatus> statuses = null
        ) {
            Mock.Setup(x => x.GetHostedServiceStatuses())
                .Returns(statuses ?? System.Array.Empty<ServiceInstanceStatus>());
        }

        public void Setup_Address(string address) {
            Mock.SetupGet(x => x.Address).Returns(address);
        }

        public void Setup_GetCapabilities() {
            Mock.Setup(x => x.GetCapabilities()).Returns(new[] { "" });
        }

        public void Setup_HasHostedServices(bool hasHostServices) {
            Mock.SetupGet(x => x.HasHostedServices).Returns(hasHostServices);
        }

        public void Setup_GetHostedServices(IEnumerable<IReadOnlyServiceRegistration> serviceRegistrations = null) {
            serviceRegistrations ??= System.Array.Empty<IReadOnlyServiceRegistration>();
            Mock.Setup(x => x.GetHostedServices()).Returns(serviceRegistrations);
        }

        public void Setup_HostRunState(RunStateEnum runState = RunStateEnum.Active) {
            Mock.SetupGet(x => x.HostRunState).Returns(runState);
        }

        public void Setup_FabricId(string hostId) {
            Mock.SetupGet(x => x.FabricId).Returns(hostId);
        }

        public void Setup_Dependencies(IEnumerable<IReadOnlyDescriptor> dependencies) {
            Mock.Setup(x => x.GetDependencies()).Returns(dependencies);
        }
    }
}

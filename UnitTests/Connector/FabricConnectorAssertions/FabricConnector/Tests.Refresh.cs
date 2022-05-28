using System.Linq;
using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Registration;
using RegistryOperationNames = XKit.Lib.Common.Services.StandardConstants.Managed.StandardServices.Registry.Operations;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Services.Registry;

namespace UnitTests.Connector.FabricConnectorAssertions.FabricConnector {

    [TestClass]
    public partial class Refresh : FabricConnectorTestsCommon {
        
        [TestMethod]
        public async Task ResetsDependencies() {
            
            // -----------------------------------------------------------------
            // Arrange

            var target = CreateTarget();
            var newDependencyRegistration = CreateServiceRegistration(
                serviceName: TestConstants.DependencyName1,
                hostFabricId: TestConstants.FakeServiceHostId1,
                TestConstants.FakeServiceHostAddress2
            );
            Setup_Registry();

            RegistryClient.Setup_Refresh(
                new ServiceTopologyMap {
                    CacheExpiration = null,
                    Services = new System.Collections.Generic.List<ServiceRegistration> { newDependencyRegistration }
                }
            );

            await PrepareTarget_InitializeAndRegister(target);
            HostEnvironment.Setup_GetHealth();
            HostEnvironment.Setup_GetHostedServices();
            HostEnvironment.Setup_GetHostedServiceStatuses();

            // -----------------------------------------------------------------
            // Act

            await target.Refresh(null);

            // -----------------------------------------------------------------
            // Assert

            var targetAsConcrete = target as XKit.Lib.Connector.Fabric.FabricConnector;
            var registration = targetAsConcrete.GetDependencyRegistrations().Single();
            registration.Should().BeEquivalentTo(newDependencyRegistration);
        }
        
        [TestMethod]
        public async Task SucceedsWithUpdatedHostStatus() {
            
            // -----------------------------------------------------------------
            // Arrange

            var target = CreateTarget();
            var updatedServiceStatus = new[] {
                new ServiceInstanceStatus()
            };

            var newDependencyRegistration = CreateServiceRegistration(
                serviceName: TestConstants.DependencyName1,
                hostFabricId: TestConstants.FakeServiceHostId2,
                TestConstants.FakeServiceHostAddress2
            );
            Setup_Registry();

            RegistryClient.Setup_Refresh(
                new ServiceTopologyMap {
                    CacheExpiration = null,
                    Services = new System.Collections.Generic.List<ServiceRegistration> { newDependencyRegistration }
                }
            );

            await PrepareTarget_InitializeAndRegister(target);

            HostEnvironment.Setup_GetHostedServiceStatuses(updatedServiceStatus);
            HostEnvironment.Setup_GetHealth(HealthEnum.Moderate);
            HostEnvironment.Setup_HostRunState();
            
            // -----------------------------------------------------------------
            // Act

            await target.Refresh(null);

            // -----------------------------------------------------------------
            // Assert

            var targetAsConcrete = target as XKit.Lib.Connector.Fabric.FabricConnector;
            var registration = targetAsConcrete.GetDependencyRegistrations().Single();
            registration.Should().BeEquivalentTo(newDependencyRegistration);
        }
                
        [TestMethod]
        public async Task DoesNothingIfRegistryCallFails() {
            
            // -----------------------------------------------------------------
            // Arrange

            var target = CreateTarget();

            var newDependencyRegistration = CreateServiceRegistration(
                serviceName: TestConstants.DependencyName1,
                hostFabricId: TestConstants.FakeServiceHostId1,
                TestConstants.FakeServiceHostAddress1
            );

            var registrationCallRouter = Setup_Registry();

            registrationCallRouter.Setup_ExecuteCall<RefreshRegistrationRequest, ServiceTopologyMap>(
                req => req.OperationName == RegistryOperationNames.Refresh,
                new XKit.Lib.Common.Fabric.ServiceCallResult<ServiceTopologyMap> {
                    ServiceCallStatus = ServiceCallStatusEnum.Completed,
                    OperationStatus = LogResultStatusEnum.NonRetriableError
                }
            );

            await PrepareTarget_InitializeAndRegister(target);
            HostEnvironment.Setup_GetHealth();
            HostEnvironment.Setup_GetHostedServices();
            HostEnvironment.Setup_GetHostedServiceStatuses();

            // -----------------------------------------------------------------
            // Act

            await target.Refresh(null);

            // -----------------------------------------------------------------
            // Assert

            var targetAsConcrete = target as XKit.Lib.Connector.Fabric.FabricConnector;
            var registrations = targetAsConcrete.GetDependencyRegistrations();
            registrations.Count().Should().Be(3);       // the original three
        }
    }
}

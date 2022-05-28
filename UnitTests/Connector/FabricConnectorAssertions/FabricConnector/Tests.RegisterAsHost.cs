using System.Linq;
using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using ServiceConstants = XKit.Lib.Common.Services.StandardConstants;
using XKit.Lib.Common.Registration;
using RegistryOperationNames = XKit.Lib.Common.Services.StandardConstants.Managed.StandardServices.Registry.Operations;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.MockWrapper;
using XKit.Lib.Common.Log;

namespace UnitTests.Connector.FabricConnectorAssertions.FabricConnector {

    [TestClass]
    public class RegisterAsHost : FabricConnectorTestsCommon {
        
        [TestMethod]
        public async Task RegistersWithRegistryService() {
            
            // -----------------------------------------------------------------
            // Arrange

            var target = CreateTarget();

            var registryInstanceClient1 = Mocks.CreateWrapper<InstanceClientMockWrapper>();
            var registryInstanceClient2 = Mocks.CreateWrapper<InstanceClientMockWrapper>();
            var fakeServiceRegistration = CreateServiceRegistration(
                serviceName: TestConstants.ServiceName1,
                hostFabricId: CreateRandomString(),
                TestConstants.FakeServiceHostAddress1
            );

            InstanceClientFactory.Setup_InitializeFactory();
            InstanceClientFactory.Setup_TryCreateClient(
                TestConstants.FakeServiceHostAddress1,
                ServiceConstants.Managed.StandardServices.Registry.Descriptor,
                registryInstanceClient1
            );
            InstanceClientFactory.Setup_TryCreateClient(
                TestConstants.FakeServiceHostAddress2,
                ServiceConstants.Managed.StandardServices.Registry.Descriptor,
                registryInstanceClient2
            );

            var callRouter = ServiceCallRouterFactory.Setup_Create(
                ServiceConstants.Managed.StandardServices.Registry.Descriptor,
                new[] { registryInstanceClient1, registryInstanceClient2 },
                serviceCallRouter: Mocks.CreateWrapper<ServiceCallRouterMockWrapper>()
            );

            RegistryClient.Setup_Register(
                new ServiceTopologyMap {
                    CacheExpiration = null,
                    Services = new System.Collections.Generic.List<ServiceRegistration> { fakeServiceRegistration }
                }
            );

            var fabricId = target.Initialize();
            LocalEnvironment.SetupAll(
                HostEnvironment,
                fabricId
            );
            HostEnvironment.SetupAll(TestConstants.FakeLocalHostAddress);

            // -----------------------------------------------------------------
            // Act

            await target.Register(
                null,
                new[] { TestConstants.FakeServiceHostAddress1, TestConstants.FakeServiceHostAddress2 },
                LocalEnvironment.Object
            );

            // -----------------------------------------------------------------
            // Assert

            var targetAsConcrete = target as XKit.Lib.Connector.Fabric.FabricConnector;
            var registration = targetAsConcrete.GetDependencyRegistrations().Single();
            registration.Should().BeEquivalentTo(fakeServiceRegistration, opt => opt.ExcludingMissingMembers());
        }

        [TestMethod]
        public async Task MatchesHostAddressToFindLocal() {

            // -----------------------------------------------------------------
            // Arrange

            var target = CreateTarget();

            var registryInstanceClient1 = Mocks.CreateWrapper<InstanceClientMockWrapper>();
            InstanceClientFactory.Setup_InitializeFactory();
            InstanceClientFactory.Setup_TryCreateClient(
                TestConstants.FakeLocalHostAddress,
                ServiceConstants.Managed.StandardServices.Registry.Descriptor,
                registryInstanceClient1
            );
            var callRouter = ServiceCallRouterFactory.Setup_Create(
                ServiceConstants.Managed.StandardServices.Registry.Descriptor,
                new[] { registryInstanceClient1 },
                serviceCallRouter: Mocks.CreateWrapper<ServiceCallRouterMockWrapper>()
            );
            callRouter.Setup_ExecuteCall<FabricRegistration, ServiceTopologyMap>(
                req => req.OperationName == RegistryOperationNames.Register,
                new XKit.Lib.Common.Fabric.ServiceCallResult<ServiceTopologyMap> {
                    ServiceCallStatus = ServiceCallStatusEnum.Completed,
                    OperationStatus = LogResultStatusEnum.NonRetriableError   // Simulate failure of registry call so
                                                                              // we preserve the original state to examine
                }
            );

            var fabricId = target.Initialize();
            LocalEnvironment.SetupAll(
                HostEnvironment,
                fabricId,
                dependencies: new [] { TestConstants.Dependency1 }
            );
            HostEnvironment.SetupAll(
                FabricConnectorAssertions.TestConstants.FakeLocalHostAddress,
                hostedServices: new[] {
                    CreateServiceRegistration(fabricId, TestConstants.Dependency1, TestConstants.FakeLocalHostAddress ),
                    CreateServiceRegistration(fabricId, TestConstants.Dependency2, TestConstants.FakeLocalHostAddress )
                }
            );

            // -----------------------------------------------------------------
            // Act

            await target.Register(
                null,
                new[] { TestConstants.FakeLocalHostAddress },
                LocalEnvironment.Object
            );

            // -----------------------------------------------------------------
            // Assert

            // Registrations should include the hosted services + the registry
            // and the registry should point locally.
            var targetAsConcrete = target as XKit.Lib.Connector.Fabric.FabricConnector;
            var registrations = targetAsConcrete.GetDependencyRegistrations();
            registrations.Should().Contain(r => r.Descriptor.Name == TestConstants.Dependency1.Name);
            registrations.Should().Contain(r => r.Descriptor.Name == TestConstants.Dependency2.Name);
            registrations.Should().Contain(r => r.Descriptor.Name == ServiceConstants.Managed.StandardServices.Registry.Descriptor.Name);
            registrations
                .Single(r => r.Descriptor.Name == ServiceConstants.Managed.StandardServices.Registry.Descriptor.Name)
                .Instances
                .Single()
                .HostFabricId.Should().Be(target.FabricId);
        }

        [TestMethod]
        public async Task MatchesLocalHostFlag() {
            // -----------------------------------------------------------------
            // Arrange

            var target = CreateTarget();

            var registryInstanceClient1 = Mocks.CreateWrapper<InstanceClientMockWrapper>();
            InstanceClientFactory.Setup_InitializeFactory();
            InstanceClientFactory.Setup_TryCreateClient(
                TestConstants.FakeLocalHostAddress,
                ServiceConstants.Managed.StandardServices.Registry.Descriptor,
                registryInstanceClient1
            );
            var callRouter = ServiceCallRouterFactory.Setup_Create(
                ServiceConstants.Managed.StandardServices.Registry.Descriptor,
                new[] { registryInstanceClient1 },
                serviceCallRouter: Mocks.CreateWrapper<ServiceCallRouterMockWrapper>()
            );
            callRouter.Setup_ExecuteCall<FabricRegistration, ServiceTopologyMap>(
                req => req.OperationName == RegistryOperationNames.Register,
                new XKit.Lib.Common.Fabric.ServiceCallResult<ServiceTopologyMap> {
                    ServiceCallStatus = ServiceCallStatusEnum.Completed,
                    OperationStatus = LogResultStatusEnum.NonRetriableError // Simulate failure of registry call so
                                                                              // we preserve the original state to examine
                }
            );

            var fabricId = target.Initialize();
            LocalEnvironment.SetupAll(
                HostEnvironment,
                fabricId,
                dependencies: new [] { TestConstants.Dependency1 }
            );
            HostEnvironment.SetupAll(
                FabricConnectorAssertions.TestConstants.FakeLocalHostAddress,
                hostedServices: new[] {
                    CreateServiceRegistration(fabricId, TestConstants.Dependency1, TestConstants.FakeLocalHostAddress ),
                    CreateServiceRegistration(fabricId, TestConstants.Dependency2, TestConstants.FakeLocalHostAddress )
                }
            );
            
            // -----------------------------------------------------------------
            // Act

            await target.Register(
                null,
                new[] { XKit.Lib.Common.Host.HostConstants.LocalHostAddressFlag },
                LocalEnvironment.Object
            );

            // -----------------------------------------------------------------
            // Assert

            // Registrations should include the hosted services + the registry
            // and the registry should point locally.
            var targetAsConcrete = target as XKit.Lib.Connector.Fabric.FabricConnector;
            var registrations = targetAsConcrete.GetDependencyRegistrations();
            registrations.Should().Contain(r => r.Descriptor.Name == TestConstants.Dependency1.Name);
            registrations.Should().Contain(r => r.Descriptor.Name == TestConstants.Dependency2.Name);
            registrations.Should().Contain(r => r.Descriptor.Name == ServiceConstants.Managed.StandardServices.Registry.Descriptor.Name);
            var registryRegistration = registrations
                .Single(r => r.Descriptor.Name == ServiceConstants.Managed.StandardServices.Registry.Descriptor.Name)
                .Instances
                .Single();
            registryRegistration.HostFabricId.Should().Be(target.FabricId);
            registryRegistration.HostAddress.Should().Be(TestConstants.FakeLocalHostAddress);
        }

        [TestMethod]    
        public async Task FallsBackOnLocalServicesIfRegistryFails() {
            
            // -----------------------------------------------------------------
            // Arrange

            var target = CreateTarget();

            var registryInstanceClient1 = Mocks.CreateWrapper<InstanceClientMockWrapper>();
            InstanceClientFactory.Setup_InitializeFactory();
            InstanceClientFactory.Setup_TryCreateClient(
                TestConstants.FakeServiceHostAddress1,
                ServiceConstants.Managed.StandardServices.Registry.Descriptor,
                registryInstanceClient1
            );
            var callRouter = ServiceCallRouterFactory.Setup_Create(
                ServiceConstants.Managed.StandardServices.Registry.Descriptor,
                new[] { registryInstanceClient1 },
                serviceCallRouter: Mocks.CreateWrapper<ServiceCallRouterMockWrapper>()
            );
            
            var fabricId = target.Initialize();
            LocalEnvironment.SetupAll(
                HostEnvironment,
                fabricId,
                dependencies: new [] { TestConstants.Dependency1 }
            );
            HostEnvironment.SetupAll(
                FabricConnectorAssertions.TestConstants.FakeLocalHostAddress,
                hostedServices: new[] { 
                    // The same host has the dependency
                    CreateServiceRegistration(fabricId, TestConstants.Dependency1, TestConstants.FakeLocalHostAddress ),
                    CreateServiceRegistration(fabricId, TestConstants.Dependency2, TestConstants.FakeLocalHostAddress )
                }
            );

            
            // -----------------------------------------------------------------
            // Act

            await target.Register(
                null,
                new[] { TestConstants.FakeServiceHostAddress1 },
                LocalEnvironment.Object
            );

            // -----------------------------------------------------------------
            // Assert

            // Registrations should include the hosted services + the registry
            // and the registry should point locally.
            var targetAsConcrete = target as XKit.Lib.Connector.Fabric.FabricConnector;
            var registrations = targetAsConcrete.GetDependencyRegistrations();
            registrations.Should().Contain(r => r.Descriptor.Name == TestConstants.Dependency1.Name);
            registrations.Should().Contain(r => r.Descriptor.Name == TestConstants.Dependency2.Name);
            registrations.Should().Contain(r => r.Descriptor.Name == ServiceConstants.Managed.StandardServices.Registry.Descriptor.Name);
        }
        
        [TestMethod]
        public async Task FallsBackOnLocalServicesIfNoRegisterCallRouter() {

            // -----------------------------------------------------------------
            // Arrange

            var target = CreateTarget();
            var registryInstanceClient1 = Mocks.CreateWrapper<InstanceClientMockWrapper>();

            InstanceClientFactory.Setup_InitializeFactory();
            InstanceClientFactory.Setup_TryCreateClient(
                TestConstants.FakeServiceHostAddress1,
                ServiceConstants.Managed.StandardServices.Registry.Descriptor,
                registryInstanceClient1
            );

            ServiceCallRouterFactory.Setup_Create(
                ServiceConstants.Managed.StandardServices.Registry.Descriptor,
                new[] { registryInstanceClient1 },
                null
            );

            var hostId = target.Initialize();
            HostEnvironment.Setup_Address(FabricConnectorAssertions.TestConstants.FakeLocalHostAddress);
            HostEnvironment.Setup_GetCapabilities();
            HostEnvironment.Setup_GetHealth();
            HostEnvironment.Setup_HasHostedServices(true);
            // Create a dependency on a service we have
            LocalEnvironment.SetupAll(
                HostEnvironment,
                target.FabricId,
                dependencies: new [] { TestConstants.Dependency1 }
            );
            HostEnvironment.Setup_GetHostedServices(new[] { 
                // The same host has the dependency
                CreateServiceRegistration(TestConstants.HostFabricId, TestConstants.Dependency1, TestConstants.FakeLocalHostAddress ),
                CreateServiceRegistration(TestConstants.HostFabricId, TestConstants.Dependency2, TestConstants.FakeLocalHostAddress )
            });
            
            // -----------------------------------------------------------------
            // Act

            await target.Register(
                null,
                new[] { TestConstants.FakeServiceHostAddress1 },
                LocalEnvironment.Object
            );

            // -----------------------------------------------------------------
            // Assert

            // Registrations should include the hosted services + the registry
            // and the registry should point locally.
            var targetAsConcrete = target as XKit.Lib.Connector.Fabric.FabricConnector;
            var registrations = targetAsConcrete.GetDependencyRegistrations();
            registrations.Should().Contain(r => r.Descriptor.Name == TestConstants.Dependency1.Name);
            registrations.Should().Contain(r => r.Descriptor.Name == TestConstants.Dependency2.Name);
            registrations.Should().Contain(r => r.Descriptor.Name == ServiceConstants.Managed.StandardServices.Registry.Descriptor.Name);
        }    
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using XKit.Lib.Common.Registration;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UnitTests.MockWrapper;

namespace UnitTests.Connector.FabricConnectorAssertions.FabricConnector {

    [TestClass]
    public class CreateRouter : FabricConnectorTestsCommon {
        
        [TestMethod]
        public async Task ObtainsRouterToManagedService() {
            
            // -----------------------------------------------------------------
            // Arrange

            var target = CreateTarget();
            Setup_Registry();

            InstanceClientFactory.Setup_TryCreateClient(
                TestConstants.FakeServiceHostAddress1,
                TestConstants.Dependency1,
                new InstanceClientMockWrapper()
            );
            var callRouter = ServiceCallRouterFactory.Setup_Create(
                TestConstants.Dependency1,
                mockClients: null,
                this.Mocks.CreateWrapper<ServiceCallRouterMockWrapper>()
            );
            await PrepareTarget_InitializeAndRegister(target);

            // -----------------------------------------------------------------
            // Act

            var result = await target.CreateCallRouter(
                target: TestConstants.Dependency1,
                log: null
            );

            // -----------------------------------------------------------------
            // Assert

            result.Should().Be(callRouter.Object);
        }

        [TestMethod]
        public async Task ObtainsRouterToMetaService() {
            
            // -----------------------------------------------------------------
            // Arrange

            var target = CreateTarget();
            Setup_Registry();

            InstanceClientFactory.Setup_TryCreateClient(
                TestConstants.FakeServiceHostAddress1,
                TestConstants.MetaServiceDependency,
                new InstanceClientMockWrapper()
            );
            var callRouter = ServiceCallRouterFactory.Setup_Create(
                TestConstants.MetaServiceDependency,
                mockClients: null,
                this.Mocks.CreateWrapper<ServiceCallRouterMockWrapper>()
            );
            await PrepareTarget_InitializeAndRegister(target);

            // -----------------------------------------------------------------
            // Act

            var result = await target.CreateCallRouter(
                target: TestConstants.MetaServiceDependency,
                log: null
            );

            // -----------------------------------------------------------------
            // Assert

            result.Should().Be(callRouter.Object);
            //Mocks.VerifyAll();
        }

        // NOTE:  Broken test 
        // [TestMethod]
        // public async Task ThrowsWhenFatalIfNotAvailableTrue() {

        //     // -----------------------------------------------------------------
        //     // Arrange

        //     var target = CreateTarget();
        //     var registrationCallRouter = Setup_Registry();

        //     InstanceClientFactory.Setup_TryCreateClient(
        //         TestConstants.FakeServiceHostAddress1,
        //         TestConstants.Dependency1,
        //         null
        //     );
        //     await PrepareTarget_InitializeAndRegister(target);

        //     // -----------------------------------------------------------------
        //     // Act

        //     await target.Awaiting(async t => await t.CreateRouter(
        //         CreateCallContext(),
        //         TestConstants.Dependency1,
        //         failIfNotAvailable: true
        //     )).Should().ThrowAsync<Exception>();

        //     // -----------------------------------------------------------------
        //     // Assert

        //     Mocks.VerifyAll();
        // }

        [TestMethod]
        public async Task DoesNotThrowWhenFatalIfNotAvailableFalse() {
            
            // -----------------------------------------------------------------
            // Arrange

            var target = CreateTarget();
            var registrationCallRouter = Setup_Registry();

            InstanceClientFactory.Setup_TryCreateClient(
                TestConstants.FakeServiceHostAddress1,
                TestConstants.Dependency1,
                null
            );
            await PrepareTarget_InitializeAndRegister(target);

            // -----------------------------------------------------------------
            // Act

            var result = await target.CreateCallRouter(
                target: TestConstants.Dependency1,
                log: null,
                failIfNotAvailable: false
            );

            // -----------------------------------------------------------------
            // Assert

            result.Should().BeNull();
        }

        [TestMethod]
        public async Task RefreshesOnCacheExpired() {
            // -----------------------------------------------------------------
            // Arrange

            var target = CreateTarget();
            Setup_Registry(cacheExpiration: DateTime.Now.AddSeconds(3));

            InstanceClientFactory.Setup_TryCreateClient(
                TestConstants.FakeServiceHostAddress1,
                TestConstants.Dependency1,
                new InstanceClientMockWrapper()
            );

            var callRouter = ServiceCallRouterFactory.Setup_Create(
                TestConstants.Dependency1,
                mockClients: null,
                this.Mocks.CreateWrapper<ServiceCallRouterMockWrapper>()
            );
            
            var newDependencyRegistration = CreateServiceRegistration(
                serviceName: TestConstants.DependencyName1,
                hostFabricId: TestConstants.FakeServiceHostId1,
                TestConstants.FakeServiceHostAddress1
            );

            // registrationCallRouter.Setup_ExecuteCall<RefreshRegistrationRequest, ServiceTopologyMap>(
            //     req => req.OperationName == RegistryOperationNames.Refresh,
            //     new XKit.Lib.Common.Fabric.ServiceCallResult<ServiceTopologyMap> {
            //         ServiceCallStatus = ServiceCallStatusEnum.Completed,
            //         OperationStatus = LogResultStatusEnum.Success,
            //         ResponseBody = new ServiceTopologyMap {
            //             CacheExpiration = null,
            //             Services = new System.Collections.Generic.List<ServiceRegistration> { newDependencyRegistration }
            //         }
            //     }
            // );
            RegistryClient.Setup_Refresh(
                new ServiceTopologyMap {
                    CacheExpiration = null,
                    Services = new System.Collections.Generic.List<ServiceRegistration> { newDependencyRegistration }
                }            
            );

            HostEnvironment.Setup_GetHealth();
            HostEnvironment.Setup_GetHostedServiceStatuses(null);
            await PrepareTarget_InitializeAndRegister(target);

            Thread.Sleep(3500);    // let cache time out

            // -----------------------------------------------------------------
            // Act

            var result = await target.CreateCallRouter(
                target: TestConstants.Dependency1,
                log: null,
                failIfNotAvailable: false
            );

            // -----------------------------------------------------------------
            // Assert

            result.Should().Be(callRouter.Object);
        }

        [TestMethod]
        public async Task ResolvesForNewDependency() {
            
            // -----------------------------------------------------------------
            // Arrange

            var target = CreateTarget();
            Setup_Registry();

            InstanceClientFactory.Setup_TryCreateClient(
                TestConstants.FakeServiceHostAddress1,
                TestConstants.Dependency2,
                new InstanceClientMockWrapper()
            );
            var callRouter = ServiceCallRouterFactory.Setup_Create(
                TestConstants.Dependency2,
                mockClients: null,
                this.Mocks.CreateWrapper<ServiceCallRouterMockWrapper>()
            );

            var newDependencyRegistration = CreateServiceRegistration(
                serviceName: TestConstants.DependencyName2,
                hostFabricId: TestConstants.FakeServiceHostId1,
                TestConstants.FakeServiceHostAddress1
            );
            // registrationCallRouter.Setup_ExecuteCall<RefreshRegistrationRequest, ServiceTopologyMap>(
            //     req => req.OperationName == RegistryOperationNames.Refresh,
            //     new XKit.Lib.Common.Fabric.ServiceCallResult<ServiceTopologyMap> {
            //         ServiceCallStatus = ServiceCallStatusEnum.Completed,
            //         OperationStatus = LogResultStatusEnum.Success,
            //         ResponseBody = new ServiceTopologyMap {
            //             CacheExpiration = null,
            //             Services = new System.Collections.Generic.List<ServiceRegistration> { newDependencyRegistration }
            //         }
            //     }
            // );
            RegistryClient.Setup_Refresh(
                new ServiceTopologyMap {
                    CacheExpiration = null,
                    Services = new System.Collections.Generic.List<ServiceRegistration> { newDependencyRegistration }
                }            
            );

            HostEnvironment.Setup_GetHealth();
            HostEnvironment.Setup_GetHostedServiceStatuses(null);

            await PrepareTarget_InitializeAndRegister(target);

            // -----------------------------------------------------------------
            // Act

            var result = await target.CreateCallRouter(
                target: TestConstants.Dependency2,
                log: null,
                failIfNotAvailable: false
            );

            // -----------------------------------------------------------------
            // Assert

            result.Should().Be(callRouter.Object);
        }

        [TestMethod]
        public async Task CachesRouter() {
            
            // -----------------------------------------------------------------
            // Arrange

            var target = CreateTarget();
            var registrationCallRouter = Setup_Registry();

            InstanceClientFactory.Setup_TryCreateClient(
                TestConstants.FakeServiceHostAddress1,
                TestConstants.Dependency1,
                new InstanceClientMockWrapper()
            );
            var callRouter = ServiceCallRouterFactory.Setup_Create(
                TestConstants.Dependency1,
                mockClients: null,
                this.Mocks.CreateWrapper<ServiceCallRouterMockWrapper>()
            );
            await PrepareTarget_InitializeAndRegister(target);
            
            // First call gets everything, next one should be cached
            await target.CreateCallRouter(
                target: TestConstants.Dependency1,
                log: null,
                failIfNotAvailable: false
            );

            // -----------------------------------------------------------------
            // Act

            var result = await target.CreateCallRouter(
                target: TestConstants.Dependency1,
                log: null,
                failIfNotAvailable: false
            );

            // -----------------------------------------------------------------
            // Assert

            result.Should().Be(callRouter.Object);
            ServiceCallRouterFactory.Verify_Create(TestConstants.Dependency1, Times.Once);
        }
    }
}

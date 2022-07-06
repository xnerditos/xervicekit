using System;
using System.Collections.Generic;
using System.Linq;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Registration;
using UnitTests.MockWrapper;
using System.Threading.Tasks;
using XKit.Lib.Common.Utility;
using ServiceConstants = XKit.Lib.Common.Services.StandardConstants;

namespace UnitTests.Connector.FabricConnectorAssertions {

    public partial class FabricConnectorTestsBase : TestBase {


        // =====================================================================
        // Common mocks
        // =====================================================================
        protected ServiceCallFactoryMockWrapper ServiceCallRouterFactory { get; private set; } 
        protected InstanceClientFactoryMockWrapper InstanceClientFactory { get; private set; }
        protected XKitEnvironmentMockWrapper XKitEnvironment { get; private set; }
        protected HostEnvironmentMockWrapper HostEnvironment { get; private set; }
        protected RegistryClientMock RegistryClient { get; private set; }

        // =====================================================================
        // Init and cleanup
        // =====================================================================

        public FabricConnectorTestsBase() { 
            ServiceCallRouterFactory = Mocks.CreateWrapper<ServiceCallFactoryMockWrapper>();
            InstanceClientFactory = Mocks.CreateWrapper<InstanceClientFactoryMockWrapper>();
            XKitEnvironment = Mocks.CreateWrapper<XKitEnvironmentMockWrapper>();
            HostEnvironment = Mocks.CreateWrapper<HostEnvironmentMockWrapper>();
            RegistryClient = new RegistryClientMock();
        }

        // =====================================================================
        // object creation
        // =====================================================================

        protected IFabricConnector CreateTarget(
        ) => new XKit.Lib.Connector.Fabric.FabricConnector(
                ServiceCallRouterFactory.Object,
                new[] { InstanceClientFactory.Object },
                RegistryClient
            );
        protected static FabricRegistration CreateHostRegistration(
            string hostId,
            IEnumerable<ServiceRegistration> serviceRegistrations = null
        ) => new() {
            Dependencies = new List<Descriptor> { TestConstants.Dependency1 },
            FabricId = hostId,
            Capabilities = null,
            Address = TestConstants.FakeLocalHostAddress,
            Status = new FabricStatus { 
                FabricId = TestConstants.HostFabricId, 
                Health = HealthEnum.Healthy, 
                RunState = RunStateEnum.Active 
            },
            HostedServices = (serviceRegistrations ?? new[] {
                CreateServiceRegistration(
                    TestConstants.LocalServiceName1,
                    TestConstants.HostFabricId,
                    TestConstants.FakeLocalHostAddress
                ),
                CreateServiceRegistration(
                    TestConstants.LocalServiceName2,
                    TestConstants.HostFabricId,
                    TestConstants.FakeLocalHostAddress
                ),  
            }).ToList()
        };

        public static ServiceRegistration CreateServiceRegistration(
            string serviceName,
            string hostFabricId,
            params string[] hostAddresses
        ) {
            var descriptor = new Descriptor {
                    Collection = TestConstants.CollectionName,
                    Name = serviceName,
                    Version = 1
            };
            return CreateServiceRegistration(
                hostFabricId,
                descriptor,
                hostAddresses
            );
        }

        protected static ServiceRegistration CreateServiceRegistration(
            string hostFabricId,
            Descriptor descriptor,
            params string[] hostAddresses
        ) {
            var registrationKey = 
                XKit.Lib.Common.Utility.Identifiers.GetServiceFullRegistrationKey(descriptor);

            var instances = new List<ServiceInstance>();

            instances.AddRange(hostAddresses.Select(address => 
                new ServiceInstance {
                    Descriptor = descriptor,
                    HostFabricId = hostFabricId,
                    HostAddress = address,
                    InstanceId = $"{descriptor.Name}-instance-for-{address}",
                    RegistrationKey = registrationKey
                }
            ));

            return new ServiceRegistration {
                Descriptor = descriptor,
                Instances = instances,
                CallPolicy = new ServiceCallPolicy(),
                RegistrationKey = registrationKey
            };
        }

        // =====================================================================
        // Setup and prepare
        // =====================================================================

        /// <summary>
        /// Sets up for the InitializeLocalTestHost and Register calls to be made
        /// </summary>
        /// <returns></returns>
        protected ServiceCallRouterMockWrapper Setup_Registry(
            IEnumerable<ServiceRegistration> initialServiceRegistrationsForDependencies = null,
            IEnumerable<ServiceRegistration> hostedServices = null,
            DateTime? cacheExpiration = null
        ) {

            var registryInstanceClient = Mocks.CreateWrapper<InstanceClientMockWrapper>();
            if (initialServiceRegistrationsForDependencies == null) { 
                var dependency1Registration = CreateServiceRegistration(
                    serviceName: TestConstants.DependencyName1,
                    hostFabricId: TestConstants.FakeServiceHostId1,
                    TestConstants.FakeServiceHostAddress1
                );

                var metaRegistration = CreateServiceRegistration(
                    hostFabricId: TestConstants.FakeServiceHostId1,
                    descriptor: new Descriptor {
                        Collection = "Meta",
                        Name = TestConstants.MetaDependencyName,
                        Version = 1,
                        IsMetaService = true
                    },
                    TestConstants.FakeServiceHostAddress1
                );
                
                initialServiceRegistrationsForDependencies = new[] { 
                    dependency1Registration,
                    metaRegistration
                };
            }
            HostEnvironment.SetupAll(
                TestConstants.FakeLocalHostAddress,
                hostedServices: hostedServices,
                fabricId: TestConstants.HostFabricId,
                dependencies: initialServiceRegistrationsForDependencies.Select(sv => sv.Descriptor)              
            );

            var registrationInstanceId = CreateRandomString();
            var registryServiceDependencyRegistration = new ServiceRegistration {
                Descriptor = ServiceConstants.Managed.StandardServices.Registry.Descriptor.Clone(),
                Instances = new List<ServiceInstance> { new ServiceInstance { 
                    Descriptor = ServiceConstants.Managed.StandardServices.Registry.Descriptor.Clone(),
                    HostFabricId = TestConstants.FakeServiceHostId1,
                    HostAddress = TestConstants.FakeServiceHostAddress1,
                    InstanceId = registrationInstanceId,
                    RegistrationKey = Identifiers.GetServiceFullRegistrationKey(ServiceConstants.Managed.StandardServices.Registry.Descriptor),
                    Status = new ServiceInstanceStatus {
                        Availability = AvailabilityEnum.Serving5,
                        Health = HealthEnum.Healthy,
                        InstanceId = registrationInstanceId,
                        RunState = RunStateEnum.Active
                    }
                }}
            };

            InstanceClientFactory.Setup_InitializeFactory();
            InstanceClientFactory.Setup_TryCreateClient(
                TestConstants.FakeServiceHostAddress1,
                ServiceConstants.Managed.StandardServices.Registry.Descriptor,
                registryInstanceClient
            );

            var registrationCallRouter = ServiceCallRouterFactory.Setup_Create(
                ServiceConstants.Managed.StandardServices.Registry.Descriptor,
                new[] { registryInstanceClient },
                serviceCallRouter: Mocks.CreateWrapper<ServiceCallRouterMockWrapper>()
            );

            RegistryClient.Setup_Register(
                new ServiceTopologyMap {
                    CacheExpiration = cacheExpiration,
                    Services = 
                        initialServiceRegistrationsForDependencies
                        .Union(new[] { registryServiceDependencyRegistration })
                        .ToList()
                }
            );
            // registrationCallRouter.Setup_ExecuteCall<FabricRegistration, ServiceTopologyMap>(
            //     req => req.OperationName == RegistryOperationNames.Register,
            //     new XKit.Lib.Common.Fabric.ServiceCallResult<ServiceTopologyMap> {
            //         ServiceCallStatus = ServiceCallStatusEnum.Completed,
            //         OperationStatus = LogResultStatusEnum.Success,
            //         ResponseBody = new ServiceTopologyMap {
            //             CacheExpiration = cacheExpiration,
            //             Services = 
            //                 initialServiceRegistrationsForDependencies
            //                 .Union(new[] { registryServiceDependencyRegistration })
            //                 .ToList()
            //         }
            //     }
            // );

            return registrationCallRouter;
        }

        /// <summary>
        /// Does the InitializeLocalTestHost and Register calls on the target to prepare it
        /// </summary>
        protected async Task PrepareTarget_InitializeAndRegister(IFabricConnector target) {
            var hostId = target.Initialize();
            XKitEnvironment.Setup_FabricId(hostId);
            XKitEnvironment.Setup_Dependencies();
            await target.Register(
                null,
                new[] { TestConstants.FakeServiceHostAddress1 },
                XKitEnvironment.Object
            );
        }
    }
}

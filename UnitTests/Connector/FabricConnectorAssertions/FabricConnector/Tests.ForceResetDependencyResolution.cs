using System.Linq;
using System.Threading.Tasks;
using XKit.Lib.Common.Registration;
using XKit.Lib.Connector.Fabric;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Connector.FabricConnectorAssertions.FabricConnector {

    [TestClass]
    public class ForceResetTopologyMap : FabricConnectorTestsCommon {
        
        [TestMethod]
        public async Task ResetsDependencies() {
            // -----------------------------------------------------------------
            // Arrange

            var target = CreateTarget();
            Setup_Registry();
            await PrepareTarget_InitializeAndRegister(target);
            var newServiceRegistration = CreateServiceRegistration(
                serviceName: TestConstants.ServiceName2,
                hostFabricId: target.FabricId,
                TestConstants.FakeServiceHostAddress1
            );

            var newMap = new ServiceTopologyMap {
                CacheExpiration = null,
                Services = new System.Collections.Generic.List<ServiceRegistration> { newServiceRegistration }
            };

            // -----------------------------------------------------------------
            // Act

            await target.ForceResetTopologyMap(newMap);

            // -----------------------------------------------------------------
            // Assert

            var targetAsConcrete = target as XKit.Lib.Connector.Fabric.FabricConnector;
            var registration = targetAsConcrete.GetDependencyRegistrations().Single();
            registration.Should().BeEquivalentTo(newServiceRegistration);
            //Mocks.VerifyAll();
        }
    }
}

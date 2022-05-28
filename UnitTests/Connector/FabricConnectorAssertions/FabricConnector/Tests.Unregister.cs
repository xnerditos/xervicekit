using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Connector.FabricConnectorAssertions.FabricConnector {

    [TestClass]
    public partial class Unregister : FabricConnectorTestsCommon {
        
        [TestMethod]
        public async Task UnregistersWithRegistryService() {
            
            // -----------------------------------------------------------------
            // Arrange

            var target = CreateTarget();
            Setup_Registry();

            RegistryClient.Setup_Unregister_Succeeds();

            await PrepareTarget_InitializeAndRegister(target);

            // -----------------------------------------------------------------
            // Act

            bool result = await target.Unregister(null);

            // -----------------------------------------------------------------
            // Assert
            result.Should().BeTrue();
            //Mocks.VerifyAll();
        }
    }
}

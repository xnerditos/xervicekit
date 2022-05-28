using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Connector.FabricConnectorAssertions.FabricConnector {

    [TestClass]
    public class Initialize : FabricConnectorTestsCommon {
        
        [TestMethod]
        public void Succeeds() {
            
            var target = CreateTarget();
            var hostId = target.Initialize();

            Mocks.VerifyAll();
        }
    }
}

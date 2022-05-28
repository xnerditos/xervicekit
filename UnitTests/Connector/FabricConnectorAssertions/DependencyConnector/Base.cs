using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Connector.FabricConnectorAssertions.DependencyConnectorAssertions {

    [TestClass]
    public partial class DependencyConnectorTestsBase : FabricConnectorTestsBase {

        // =====================================================================
        // create
        // =====================================================================

        protected new IDependencyConnector CreateTarget() 
            => base.CreateTarget();
    
        // =====================================================================
        // other
        // =====================================================================

        protected async Task PrepareTarget_InitializeAndRegister(IDependencyConnector target) 
            => await base.PrepareTarget_InitializeAndRegister(target as IFabricConnector);
    }
}

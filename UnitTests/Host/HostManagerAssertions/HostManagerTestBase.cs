using XKit.Lib.Common.Host;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.MockWrapper;
using System;
using XKit.Lib.Common.Registration;

namespace UnitTests.Host.XKitHostAssertions {

    [TestClass]
    public partial class XKitHostTestBase : TestBase {

        protected FabricConnectorMockWrapper FabricConnectorMockWrapper { get; private set; }
        protected LogSessionFactoryMockWrapper LogSessionFactoryMockWrapper { get; private set; }
        protected LocalConfigSessionFactoryMockWrapper LocalConfigSessionMockWrapper { get; private set; }
        protected ConfigClientMockWrapper ConfigClientWrapper { get; private set; }
        public XKitHostTestBase() {
            this.FabricConnectorMockWrapper = Mocks.CreateWrapper<FabricConnectorMockWrapper>();
            this.LogSessionFactoryMockWrapper = Mocks.CreateWrapper<LogSessionFactoryMockWrapper>();
            this.LocalConfigSessionMockWrapper = Mocks.CreateWrapper<LocalConfigSessionFactoryMockWrapper>();
            this.ConfigClientWrapper = Mocks.CreateWrapper<ConfigClientMockWrapper>();
            FabricConnectorMockWrapper.Mock.SetupGet(x => x.FabricId).Returns("fabricID");
        }

        // =====================================================================
        // create
        // =====================================================================

        protected IXKitHost CreateTarget(Func<HealthEnum> hostHealthGetter = null)  
            => new XKit.Lib.Host.Management.XKitHost(
                Constants.HostAddress,
                FabricConnectorMockWrapper.Object,
                LogSessionFactoryMockWrapper.Object,
                LocalConfigSessionMockWrapper.Object,
                Constants.LocalMetadataDbPath,
                Constants.LocalDataPath,
                hostHealthGetter ?? DefaultHealthGetter,
                null,
                null
            );
    
        // =====================================================================
        // other
        // =====================================================================

        // =====================================================================
        // private utility
        // =====================================================================

        private HealthEnum DefaultHealthGetter() => HealthEnum.Healthy;
    }
}

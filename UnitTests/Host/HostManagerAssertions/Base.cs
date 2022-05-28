using XKit.Lib.Common.Host;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XKit.Lib.Host.Management;
using UnitTests.MockWrapper;
using System;
using XKit.Lib.Common.Registration;

namespace UnitTests.Host.HostManagerAssertions {

    [TestClass]
    public partial class HostManagerTestBase : TestBase {

        protected FabricConnectorMockWrapper FabricConnectorMockWrapper { get; private set; }
        protected LogSessionFactoryMockWrapper LogSessionFactoryMockWrapper { get; private set; }
        protected LocalConfigSessionFactoryMockWrapper LocalConfigSessionMockWrapper { get; private set; }
        protected ConfigClientMockWrapper ConfigClientWrapper { get; private set; }
        public HostManagerTestBase() {
            this.FabricConnectorMockWrapper = Mocks.CreateWrapper<FabricConnectorMockWrapper>();
            this.LogSessionFactoryMockWrapper = Mocks.CreateWrapper<LogSessionFactoryMockWrapper>();
            this.LocalConfigSessionMockWrapper = Mocks.CreateWrapper<LocalConfigSessionFactoryMockWrapper>();
            this.ConfigClientWrapper = Mocks.CreateWrapper<ConfigClientMockWrapper>();
            FabricConnectorMockWrapper.Mock.SetupGet(x => x.FabricId).Returns("fabricID");
        }

        // =====================================================================
        // create
        // =====================================================================

        protected IHostManager CreateTarget(Func<HealthEnum> hostHealthGetter = null)  
            => new XKit.Lib.Host.Management.HostManager(
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

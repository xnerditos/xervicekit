using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XKit.Lib.Testing;

namespace SystemTests.Daemons.Tests {

    [TestClass]
    public class DaemonCtrl : TestBase {

        [TestInitialize]
        public void Initialize() { TestInit(); }

        [TestCleanup]
        public void Teardown() { TestTeardown(); }

        [TestMethod]
        public void DaemonIsRunning() => TestHelper.RunTest(() => {
            
            LastMessageTickValue.Should().BeGreaterThan(0);
        });

        [TestMethod]
        public void PausesAndResumesWithService() => TestHelper.RunTest(() => {
            var value = LastMessageTickValue;
            Yield(1100);
            LastMessageTickValue.Should().NotBe(value);
            value = LastMessageTickValue;
            Yield(1100);
            
            AutoMessagingService.PauseService(TestHelper.Log);
            LastMessageTickValue.Should().NotBe(value);
            value = LastMessageTickValue;
            Yield(1100);
            
            LastMessageTickValue.Should().Be(value);
            Yield(1100);
            LastMessageTickValue.Should().Be(value);
            
            AutoMessagingService.ResumeService(TestHelper.Log);
            Yield(1100);
            LastMessageTickValue.Should().NotBe(value);
        });
    }
}

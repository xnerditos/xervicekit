using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XKit.Lib.Testing;

namespace SystemTests.Daemons.Tests {

    [TestClass]
    public class DaemonCtrl : TestBase {

        [TestInitialize]
        public void Initialize() { ClassInit(); }

        [TestCleanup]
        public void Teardown() { ClassTeardown(); }

        [TestMethod]
        public void DaemonIsRunning() => TestHostHelper.RunTest(() => {
            
            LastMessageTickValue.Should().BeGreaterThan(0);
        });

        [TestMethod]
        public void PausesAndResumesWithService() => TestHostHelper.RunTest(() => {
            var value = LastMessageTickValue;
            Yield();
            LastMessageTickValue.Should().NotBe(value);
            value = LastMessageTickValue;
            Yield();
            
            AutoMessagingService.PauseService();
            LastMessageTickValue.Should().NotBe(value);
            value = LastMessageTickValue;
            Yield();
            
            LastMessageTickValue.Should().Be(value);
            Yield();
            LastMessageTickValue.Should().Be(value);
            
            AutoMessagingService.ResumeService();
            Yield();
            LastMessageTickValue.Should().NotBe(value);
        });
    }
}

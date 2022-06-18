using System;
using System.Diagnostics;
using System.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XKit.Lib.Testing;

namespace SystemTests.Daemons.Tests {

    //[TestClass]
    public class GenericTimerDaemonTests : TestBase {

        [TestInitialize]
        public void Initialize() { TestBase.ClassInit(); }

        [TestCleanup]
        public void Teardown() { TestBase.ClassTeardown(); }

        [TestMethod]
        public void DaemonIsRunning() => TestHostHelper.RunTest(() => {
            static uint getLastMessageValue() => 
                SvcWithAutoMessaging.Service.SvcWithAutoMessagingDaemonOperation.LastMessageTickValue;
            
            var testThreadId = System.Environment.CurrentManagedThreadId;
            Debug.WriteLine($"(test point 1) Test thread id {testThreadId}");

            Yield(2000);
            var value1 = getLastMessageValue();

            value1.Should().BeGreaterThan(0);
        });

        // TODO:

        [TestMethod]
        [Ignore]
        public void PausesAndResumesWithService() => TestHostHelper.RunTest(() => {
            throw new NotImplementedException();
        });

        [TestMethod]
        [Ignore]
        public void ResumesAndContinues() => TestHostHelper.RunTest(() => {
            throw new NotImplementedException();
        });

        [TestMethod]
        [Ignore]
        public void ManuallyPulses() => TestHostHelper.RunTest(() => {
            throw new NotImplementedException();
        });

        [TestMethod]
        [Ignore]
        public void ExceptionHandled() => TestHostHelper.RunTest(() => {
            throw new NotImplementedException();
        });

        [TestMethod]
        [Ignore]
        public void HandlesMultipleMessages() => TestHostHelper.RunTest(() => {
            throw new NotImplementedException();
        });

        [TestMethod]
        [Ignore]
        public void ProperlyShutsDown() => TestHostHelper.RunTest(() => {
            throw new NotImplementedException();
        });

        private void Yield(int milliseconds) {
            Thread.CurrentThread.Join(milliseconds);
        }
    }
}

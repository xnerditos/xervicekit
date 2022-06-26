using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XKit.Lib.Common.Host;
using XKit.Lib.Host.Helpers;

namespace UnitTests.Daemons; 

[TestClass]
public class DaemonTestBase : TestBase {

    protected readonly IDaemonEngine<TestMessage> engine;

    public DaemonTestBase() {
        engine = new DaemonEngine<TestMessage>();
        engine.DefaultTimerPeriodMilliseconds = 70;
    }
    protected static void Yield(int milliseconds = 100) {
        Thread.Sleep(milliseconds);
    }

    protected void Init() {
    }

    protected void Teardown() {
    }
}

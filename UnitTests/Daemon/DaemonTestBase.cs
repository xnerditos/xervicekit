using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XKit.Lib.Common.Host;
using XKit.Lib.Host.Helpers;

namespace UnitTests.Daemons; 

[TestClass]
public class DaemonTestBase : TestBase {

    protected readonly IDaemonEngine<TestMessage> engine;
    protected uint TimerPeriodMillisecondsFixed;

    public DaemonTestBase() {
        TimerPeriodMillisecondsFixed = 70;
        engine = new DaemonEngine<TestMessage>();
        engine.OnDetermineTimerPeriod = () => TimerPeriodMillisecondsFixed;
    }
    protected static void Yield(int milliseconds = 100) {
        Thread.Sleep(milliseconds);
    }

    protected static void Init() { }
    protected static void Teardown() { }
}

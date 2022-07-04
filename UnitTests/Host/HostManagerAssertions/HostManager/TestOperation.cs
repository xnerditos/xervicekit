using System;
using XKit.Lib.Common.Host;

namespace UnitTests.Host.XKitHostAssertions.XKitHost {

    public interface ITestOperation : IServiceApi {
        string TheOperation(string p);
    }

    public class TestOperation : ITestOperation {

        public string TheOperation(string p) => p;
    }
}

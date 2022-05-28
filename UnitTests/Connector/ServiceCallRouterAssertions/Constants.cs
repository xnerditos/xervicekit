
using System;
using XKit.Lib.Common.Registration;

namespace UnitTests.Connector.ServiceCallRouterAssertions {

    public static class Constants {
        public const string OperationName = "TheOperation";
        public static readonly string RandomString = Guid.NewGuid().ToString();
        public static readonly Descriptor ServiceDescriptor = new Descriptor {
            Collection = "collection",
            Name = "name",
            Version = 1,
            UpdateLevel = 0,
            PatchLevel = 0
        };
    }
}
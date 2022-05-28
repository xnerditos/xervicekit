
using XKit.Lib.Common.Registration;

namespace UnitTests.Connector.FabricConnectorAssertions {

    public static class TestConstants {
        public const string FakeLocalHostAddress = "123.123.123.123";
        public const string FakeServiceHostAddress1 = "100.100.100.100";
        public const string FakeServiceHostAddress2 = "200.200.200.200";
        public const string FakeServiceHostId1 = nameof(FakeServiceHostId1);
        public const string FakeServiceHostId2 = nameof(FakeServiceHostId2);
        public const string ServiceName1 = nameof(ServiceName1);
        public const string ServiceName2 = nameof(ServiceName2);
        public const string LocalServiceName1 = nameof(LocalServiceName1);
        public const string LocalServiceName2 = nameof(LocalServiceName2);
        public const string DependencyName1 = nameof(DependencyName1);
        public const string DependencyName2 = nameof(DependencyName2);
        public const string MetaDependencyName = nameof(MetaDependencyName);
        public const string CollectionName = nameof(CollectionName);
        public const string HostFabricId = nameof(HostFabricId);

        public static readonly Descriptor Dependency1 = new Descriptor {
            Collection = CollectionName,
            Name = DependencyName1,
            Version = 1
        };
        public static readonly Descriptor Dependency2 = new Descriptor {
            Collection = CollectionName,
            Name = DependencyName2,
            Version = 1
        };
        public static readonly Descriptor MetaServiceDependency = new Descriptor {
            Collection = "Meta",
            Name = MetaDependencyName,
            Version = 1,
            IsMetaService = true
        };
        
        public static string RequestorFabricId = nameof(RequestorFabricId);
        public static string RequestInstanceId = nameof(RequestInstanceId);
        public static string CorrelationId = nameof(CorrelationId);
    }
}

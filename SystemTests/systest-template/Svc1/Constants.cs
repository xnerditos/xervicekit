using XKit.Lib.Common.Registration;

namespace SystemTests._NAMESPACE.Svc1 {

    public static class Constants {
        public const string CollectionName = "SystemTest._NAMESPACE";
        public const string ServiceName = "Svc1";
        public const int Version = 1;
        public const int UpdateLevel = 0;
        public const int PatchLevel = 0;

        public static readonly IReadOnlyDescriptor ServiceDescriptor = new Descriptor {
            Collection = CollectionName,
            Name = ServiceName,
            Version = Version,
            UpdateLevel = UpdateLevel,
            PatchLevel = PatchLevel
        };
    }
}
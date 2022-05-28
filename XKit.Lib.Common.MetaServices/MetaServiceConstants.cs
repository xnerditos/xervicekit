using XKit.Lib.Common.Registration;

namespace XKit.Lib.Common.MetaServices {
    public static class MetaServiceConstants {

        public const int Version = 1;
        public const string VersionAsString = "1";
        public const string UrlRoot = "meta";
        public const string CollectionName = "Meta";

        public static class Services {

            public static class RegistrationsManagement {

                public static readonly IReadOnlyDescriptor Descriptor = new Descriptor {
                    IsMetaService = true,
                    Collection = CollectionName,
                    Name = ServiceName,
                    Version = Version,
                    UpdateLevel = 0,
                    PatchLevel = 0
                };

                public const string ServiceName = "RegistrationsManagement";

                public static class Operations { 
                    public const string ResetDependencies = "ResetDependencies";
                    public const string TriggerRefresh = "TriggerRefresh";
                } 
            }
        }
    }
}

using XKit.Lib.Common.Registration;

namespace XKit.Lib.Common.Services {
    public static class StandardConstants {
        
        public static class Managed {
            public const string UrlRoot = "managed";

            public static class Collections {
                public const string Blank = "_"; 
                public const string Platform = "Platform"; 
            }

            public static class StandardServices {

                public static class Registry {

                    public const int Version = 2;
                    public const string ServiceName = "Registry";

                    public static readonly IReadOnlyDescriptor Descriptor = new Descriptor {
                        Collection = Collections.Platform,
                        Name = ServiceName,
                        Version = Version
                    };

                    public static class Operations {

                        /// <summary>
                        /// Operation for a host or consumer to register itself
                        /// </summary>
                        public const string Register = "Register";

                        /// <summary>
                        /// Operation for a fabric actor to update itself
                        /// </summary>
                        public const string Refresh = "Refresh";

                        /// <summary>
                        /// Operation for a fabric actor to unregister itself
                        /// </summary>
                        public const string Unregister = "Unregister";
                    }
                }

                public static class Config {

                    public const int Version = 2;
                    public const string ServiceName = "Configuration";

                    public static readonly IReadOnlyDescriptor Descriptor = new Descriptor {
                        Collection = Collections.Platform,
                        Name = ServiceName,
                        Version = Version
                    };

                    public static class Operations {
                        public const string QueryConfig = "QueryConfig";
                    }
                }

                public static class MessageBroker {

                    public const int Version = 2;
                    public const string ServiceName = "MessageBroker";

                    public static readonly IReadOnlyDescriptor Descriptor = new Descriptor {
                        Collection = Collections.Platform,
                        Name = ServiceName,
                        Version = Version
                    };

                    public static class Operations {
                        public const string RaiseEvent = "RaiseEvent";
                        public const string SendCommand = "SendCommand";
                    }
                }
            }
        }
    }
}

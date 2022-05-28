namespace XKit.Lib.Common {

    public static class EnvironmentHelperConstants {
        public static class EnvironmentVariables {

            /// <summary>
            /// The externally reachable public base ADDRESS to the host
            /// </summary>
            public const string HostBaseAddress = "HOST_BASE_ADDRESS";
            
            /// <summary>
            /// The folder where local data can go.  This is "{ApplicationData}/xkit-host" by default.
            /// NOTE: ApplicationData is usually '/home/$USER/.config' on Linux
            /// </summary>
            public const string LocalDataFolderPath = "LOCAL_DATA_FOLDER_PATH";

            /// <summary>
            /// Path to the local meta data db storage file.  This is "{LocalDataFolderPath}/_metadata.db" by default.
            /// </summary>
            public const string MetaDbPath = "METADATA_DB_PATH";

            /// <summary>
            /// A list of the initial addresses to use to try to register the host.  Once the host is registered, it will have
            /// the full list of live Registry services.  Multiple ADDRESS's are separated by a semicolon ";"
            /// </summary>
            public const string InitialRegistryAddresses = "INITIAL_REGISTRY_ADDRESSES";

            /// <summary>
            /// /// The folder where configuration data goes.  This is "{ApplicationData}/xkit-host/config" by default.
            /// NOTE: ApplicationData is usually '/home/$USER/.config' on Linux
            /// </summary>
            public const string ConfigFolderPath = "CONFIG_FOLDER";
        }
    }
}

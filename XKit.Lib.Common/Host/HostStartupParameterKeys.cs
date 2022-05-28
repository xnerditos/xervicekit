namespace XKit.Lib.Common.Host {
    public static class HostStartupParameterKeys {

        /// <summary>
        /// type:  boolean     
        /// Set to false to avoid the call to the configuration service on host startup.
        /// Default = true
        /// </summary>
        public const string REFRESH_CONFIG_ON_STARTUP = "REFRESH_CONFIG_ON_STARTUP";
        /// <summary>
        /// type:  boolean     
        /// Set to false to avoid the call to the message broker service on host startup.
        /// Default = true
        /// </summary>
        public const string REGISTER_SUBSCRIPTIONS_ON_STARTUP = "REGISTER_SUBSCRIPTIONS_ON_STARTUP";
        /// <summary>
        /// type:  boolean     
        /// Set to true to throw an exception if the configuration service is not available on host startup.
        /// Default = false
        /// </summary>
        public const string FAIL_IF_CONFIG_UNAVAILABLE_ON_STARTUP = "FAIL_IF_CONFIG_UNAVAILABLE_ON_STARTUP";
        /// <summary>
        /// type:  boolean     
        /// Set to true to throw an exception if the message broker service is not available on host startup.
        /// Default = false
        /// </summary>
        public const string FAIL_IF_MESSAGE_BROKER_UNAVAILABLE_ON_STARTUP = "FAIL_IF_MESSAGE_BROKER_UNAVAILABLE_ON_STARTUP";
    }
}

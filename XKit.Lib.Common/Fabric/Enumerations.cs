namespace XKit.Lib.Common.Fabric {


    /// <summary>
    /// 
    /// </summary>
    public enum ServiceCallStatusEnum {

        /// <summary>
        /// No attempt was yet made to call the service 
        /// </summary>
        NotAttempted,

        /// <summary>
        /// A service instance was invoked and the operation completed 
        /// </summary>
        Completed,

        /// <summary>
        /// An instance of the service could not be located 
        /// </summary>
        NotFound,

        /// <summary>
        /// Service instance(s) is not accessible 
        /// </summary>
        NotAccessible,

        /// <summary>
        /// All known service instances were attempted and no successful connection was made 
        /// </summary>
        NoConnection,

        /// <summary>
        /// Error in the communication protocol itself 
        /// </summary>
        ProtocolError,

        /// <summary>
        /// The particular operation name request was not found or not implemented 
        /// </summary>
        OperationNotFound,

        /// <summary>
        /// The operation method appears to be undefined or otherwise is not correctly async
        /// </summary>
        UndefinedOperationMethod,

        /// <summary>
        /// The operation returned no result
        /// </summary>
        OperationReturnedNoResult,

        /// <summary>
        /// Something weird happened
        /// </summary>
        UnknownError,
    }

    public enum ServiceCallTypeEnum {
        
        /// <summary>
        // Service call will not return until a result is available directly
        /// </summary>
        SyncResult,

        /// <summary>
        /// Service call is made and no result is expected.  This is appropriate when operation status
        /// is relatively unimportant to the caller.
        FireAndForget,
    }
}
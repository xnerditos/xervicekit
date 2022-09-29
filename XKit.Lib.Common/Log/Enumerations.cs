namespace XKit.Lib.Common.Log {

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public enum LogEventTypeEnum {

        // =====================================================================
        // "Public" log types
        // =====================================================================

        /// <summary>
        /// Log event type is unknown
        /// </summary>
        None = 0,

        /// <summary>
        /// Entering a series of events associated with a service operation.  The code
        /// indicates the type of operation (OperationTypeEnum)
        /// </summary>
        Enter = 1,

        /// <summary>
        /// The operation is transitioning to a different running context.  
        /// </summary>
        ContextChange = 2,

        /// <summary>
        /// Unexpected condition that resulted in termination of the operation
        /// </summary>
        Fatality = 3,       
        
        /// <summary>
        /// A likely bug.  This can be followed by a warning or error
        /// </summary>
        Erratum = 4,

        /// <summary>
        /// Unexpected error for the operation, usually unrecoverable
        /// </summary>
        Error = 5,          
        
        /// <summary>
        /// Unexpected condition that is recoverable or should be noted
        /// </summary>
        Warning = 6,       
        
        /// <summary>
        /// Status message intended to communicate the progress of the operation
        /// </summary>
        Status = 7,         
        
        /// <summary>
        /// Saves a set of data that is for information
        /// </summary>
        Info = 8,
        
        /// <summary>
        /// Event that is intended to trace the execution path 
        /// </summary>
        Trace = 9,

        /// <summary>
        /// Event that records auditable activity 
        /// </summary>
        Audit = 10,

        /// <summary>
        /// Operation is returning a pending result and continuing to process 
        /// </summary>
        Pending = 98,

        /// <summary>
        /// Exiting a series of events associated with an operation
        /// </summary>
        Exit = 99,

        // =====================================================================
        // "Private" log types
        // These follow log message types are private to the service
        // =====================================================================
        
        /// <summary>
        /// Save information about the operation at a given point, intended
        /// to be available as data for the future
        /// </summary>
        Snapshot = 100,

        // =====================================================================
        // "Maintenance" log types
        // =====================================================================
        
        /// <summary>
        /// used by the logging engine to stand in for deleted entries
        /// </summary>
        LoggingEngineArchivePlaceholder = 200,

        /// <summary>
        /// used by the logging engine to stand in for deleted entries
        /// </summary>
        LoggingEngineArchiveSummary = 201,

        /// <summary>
        /// used by the logging engine to indicate a problem when doing log maintenance
        /// </summary>
        LoggingEngineWarning = 299,

        // =====================================================================
        // Service defined log type names use this LogEventTypeEnum enumeration
        // =====================================================================
        
        ServiceDefined = 1000
    }

    public enum LogOptionsEnum : uint { 
        None = 0x0000,
        IncludeEntryAndExitData = 0x0001,
        Default = IncludeEntryAndExitData,
        All = 0xffff
    }

    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public enum LogResultStatusEnum {
        /// <summary>
        /// No status determined
        /// </summary>
        Incomplete = 0,

        /// <summary>
        /// Operation completed successfully
        /// </summary>
        Success = 1,

        /// <summary>
        /// Operation completed with partial success
        /// </summary>
        PartialSuccess = 2,

        /// <summary>
        /// Operation completion is still pending
        /// </summary>
        Pending = 3,

        /// <summary>
        /// Operation work item has incorrect data
        /// </summary>
        NoAction_BadRequest = 10,

        /// <summary>
        /// The job code could not be run because it was considered unavailable
        /// </summary>
        NoAction_ServiceUnavailable = 11,

        /// <summary>
        /// The operation was attempted, but did not respond or complete in time 
        /// </summary>
        NoAction_Timeout = 13,

        /// <summary>
        /// The attempt to connect failed (operation)
        /// </summary>
        NoAction_ConnectionFailed = 14,

        /// <summary>
        /// General error which is not expected to have different results if the call is retried.  
        /// Look at code and data for details.
        /// If it is unknown whether or not an error is recoverable, the default choice should be 
        /// this one, non-retriable.
        /// </summary>
        NonRetriableError = 100,

        /// <summary>
        /// An error which might have a successful result if the service call is retried.  Look at code and data for details.
        /// </summary>
        RetriableError = 100,

        /// <summary>
        /// An exception occurred at the task level, indicating a possible problem in the toolkit
        /// itself since all such cases should be handled.
        /// </summary>
        Fault = 200,

        /// <summary>
        /// Unknown status.  Look at code and data for details.
        /// </summary>
        Unknown = 9999
    }

    public enum LogContextTypeEnum {
        OtherUnknown = 0,
        ServiceOperation = 1,
        ServiceDaemonMain = 2,
        ServiceDaemonOperation = 3,
        HostAction = 10,
        ClientAction = 20,
        DevelopmentTest = 100,
    }
}

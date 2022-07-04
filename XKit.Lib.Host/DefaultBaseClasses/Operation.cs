using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Utility.Extensions;
using XKit.Lib.Common.Host;

namespace XKit.Lib.Host.DefaultBaseClasses {

    public abstract partial class Operation : IOperation {

        private bool isActive;
        protected IXkitHostEnvironment HostEnvironment => Context.HostEnv as IXkitHostEnvironment;
        protected IFabricConnector Connector => Context.Connector;
        protected OperationContext Context { get; }
        protected ILogSession Log { get; private set; }
        protected string OperationName { get; private set; }
        
        public Operation(
            OperationContext context
        ) {
            this.Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // =====================================================================
        // abstract and virtual
        // =====================================================================

        protected abstract LogContextTypeEnum OperationType { get; }
        protected abstract string OriginatorName { get; }
        protected abstract int OriginatorVersion { get; }
        protected abstract string OriginatorInstanceId { get; }
        protected virtual bool CanStartNewOperation() => true;
        protected virtual bool IsLongRunningDefault => false;

        // =====================================================================
        // IOperationImplementor
        // =====================================================================

        bool IOperation.IsActive => this.isActive;

        // =====================================================================
        // Begin / End / Status 
        // =====================================================================

        /// <summary>
        /// Performs logic to start processing an operation
        /// </summary>
        /// <typeparam name="bool">true if operation was started successfully</typeparam>
        protected bool BeginOperation<TWorkItem>(
            TWorkItem workItem,
            string operationName,
            object additionalLogAttributes = null,
            LogOptionsEnum loggingOptions = LogOptionsEnum.Default
        ) where TWorkItem : class => BeginOperation(
            workItem,
            operationName,
            additionalLogAttributes?.FieldsToDictionary(), 
            loggingOptions
        );

        /// <summary>
        /// Performs logic to start processing an operation
        /// </summary>
        /// <typeparam name="bool">true if operation was started successfully</typeparam>
        protected bool BeginOperation<TWorkItem>(
            TWorkItem requestBody,
            string operationName,
            IReadOnlyDictionary<string, object> additionalLogAttributes,
            LogOptionsEnum loggingOptions = LogOptionsEnum.Default
        ) where TWorkItem : class {

            try {
                OperationName = operationName;
                Log = HostEnvironment.LogSessionFactory.CreateLogSession(
                    this.OriginatorName,
                    this.OriginatorVersion,
                    HostEnvironment.FabricId,
                    this.OriginatorInstanceId,
                    Context?.CorrelationId
                );
                Log.Begin(
                    this.OperationType,
                    this.OperationName,
                    (loggingOptions & LogOptionsEnum.IncludeEntryAndExitData) != LogOptionsEnum.None ? requestBody : null,
                    attributes: additionalLogAttributes
                );

                if (HostEnvironment.HostRunState == RunStateEnum.Inactive || !CanStartNewOperation()) {
                    Log.Fatality(
                        "Instance unavailable.",
                        new Dictionary<string, object> {
                            { "HostState", "HostEnvironment.HostRunState" }
                        }
                    );
                    return false;
                }

                isActive = true;
                return true;
            } catch(Exception ex) {
                isActive = false;
                Log.Erratum("Exception unexpected");
                Log.Fatality(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Creates an operation result with OperationResultStatusEnum.Pending for return and adds log entry
        /// </summary>
        /// <param name="resultMessage">accompanying result message</param>
        /// <returns>Operation result for immediate return</returns>
        protected OperationResult ResultAndLogCallPending(
            string resultMessage = null,
            string logMessage = null,
            object pendingLogParameters = null
        ) {
            if (isActive) {

                Log.Pending(
                    logMessage ?? resultMessage,
                    pendingLogParameters?.FieldsToDictionary()
                );

                return Result(
                    LogResultStatusEnum.Pending,
                    message: resultMessage
                );
            }

            var msg = "Cannot finish a call as Pending if it is not active";
            Log.Erratum(msg);
            throw new Exception(msg);
        }

        /// <summary>
        /// Creates an operation result with OperationResultStatusEnum.Pending for return and adds log entry
        /// </summary>
        /// <param name="resultMessage">accompanying result message</param>
        /// <returns>Operation result for immediate return</returns>
        protected OperationResult<TResponseBody> ResultAndLogCallPending<TResponseBody>(
            string resultMessage = null,
            string logMessage = null,
            object pendingLogParameters = null
        ) where TResponseBody : class {

            if (isActive) {

                Log.Pending(
                    logMessage ?? resultMessage,
                    pendingLogParameters?.FieldsToDictionary()
                );

                return Result<TResponseBody>(
                    LogResultStatusEnum.Pending,
                    message: resultMessage,
                    resultData: null
                );
            }

            var msg = "Cannot finish a call as Pending if it is not active";
            Log.Erratum(msg);
            throw new Exception(msg);
        }

        /// <summary>
        /// Creates an operation result with NoAction_ServiceUnavailable for return
        /// </summary>
        /// <param name="message">accompanying result message</param>
        /// <returns>Operation result for immediate return</returns>
        protected static OperationResult ResultCallInvalidServiceUnavailable(
            string message = null
        ) => Result(
                LogResultStatusEnum.NoAction_ServiceUnavailable,
                message: message
            );

        /// <summary>
        /// Creates an operation result with NoAction_ServiceUnavailable for return
        /// </summary>
        /// <param name="message">accompanying result message</param>
        /// <returns>Operation result for immediate return</returns>
        protected static OperationResult<TResponseBody> ResultCallInvalidServiceUnavailable<TResponseBody>(
            string message = null
        ) where TResponseBody : class => Result<TResponseBody>(
                LogResultStatusEnum.NoAction_ServiceUnavailable,
                null,
                message
            );

        /// <summary>
        /// Creates an operation result with OperationResultStatusEnum.Success for return
        /// </summary>
        /// <param name="operationMessage">accompanying result message</param>
        /// <returns>Operation result for immediate return</returns>
        protected static OperationResult ResultSuccess(
            string operationMessage = null,
            string logMessage = null
        ) => Result(
                LogResultStatusEnum.Success,
                message: operationMessage,
                logMessage: logMessage
            );

        /// <summary>
        /// Creates an operation result with OperationResultStatusEnum.Success for return
        /// </summary>
        /// <param name="operationMessage">accompanying result message</param>
        /// <returns>Operation result for immediate return</returns>
        protected static OperationResult<TResultData> ResultSuccess<TResultData>(
            TResultData resultData,
            string operationMessage = null,
            string logMessage = null,
            TResultData logData = null
        ) where TResultData : class => Result(
                LogResultStatusEnum.Success,
                resultData: resultData,
                message: operationMessage,
                logMessage: logMessage,
                logData: logData 
            );

        /// <summary>
        /// Creates an operation result with OperationResultStatusEnum.Success for return
        /// </summary>
        /// <param name="operationMessage">accompanying result message</param>
        /// <returns>Operation result for immediate return</returns>
        protected static OperationResult PartialResultSuccess(
            string operationMessage = null,
            string logMessage = null
        ) => Result(
                LogResultStatusEnum.PartialSuccess,
                message: operationMessage,
                logMessage: logMessage 
            );

        /// <summary>
        /// Creates an operation result with OperationResultStatusEnum.Success for return
        /// </summary>
        /// <param name="operationMessage">accompanying result message</param>
        /// <returns>Operation result for immediate return</returns>
        protected static OperationResult<TResultData> PartialResultSuccess<TResultData>(
            TResultData resultData,
            string operationMessage = null,
            string logMessage = null,
            TResultData logData = null
        ) where TResultData : class => Result(
                LogResultStatusEnum.PartialSuccess,
                resultData: resultData,
                message: operationMessage,
                logMessage: logMessage,
                logData: logData 
            );

        /// <summary>
        /// Creates an operation result with OperationResultStatusEnum.Success for return
        /// </summary>
        /// <param name="operationMessage">accompanying result message</param>
        /// <returns>Operation result for immediate return</returns>
        protected static OperationResult ResultRetriableError(
            string operationMessage = null,
            string logMessage = null
        ) => Result(
                operationStatus: LogResultStatusEnum.RetriableError,
                message: operationMessage,
                logMessage: logMessage 
            );

        /// <summary>
        /// Creates an operation result with OperationResultStatusEnum.Success for return
        /// </summary>
        /// <param name="operationMessage">accompanying result message</param>
        /// <returns>Operation result for immediate return</returns>
        protected static OperationResult<TResultData> ResultRetriableError<TResultData>(
            string operationMessage,
            string logMessage = null,
            TResultData resultData = null,
            TResultData logData = null
        ) where TResultData : class => Result(
                operationStatus: LogResultStatusEnum.RetriableError,
                resultData: resultData,
                message: operationMessage,
                logMessage: logMessage,
                logData: logData 
            );

        /// <summary>
        /// Creates an operation result with OperationResultStatusEnum.Success for return
        /// </summary>
        /// <param name="operationMessage">accompanying result message</param>
        /// <returns>Operation result for immediate return</returns>
        protected static OperationResult ResultNonRetriableError(
            string operationMessage = null,
            string logMessage = null
        ) => Result(
                operationStatus: LogResultStatusEnum.NonRetriableError,
                message: operationMessage,
                logMessage: logMessage 
            );

        /// <summary>
        /// Creates an operation result with OperationResultStatusEnum.Success for return
        /// </summary>
        /// <param name="operationMessage">accompanying result message</param>
        /// <returns>Operation result for immediate return</returns>
        protected static OperationResult<TResultData> ResultNonRetriableError<TResultData>(
            string operationMessage,
            string logMessage = null,
            TResultData resultData = null,
            TResultData logData = null
        ) where TResultData : class => Result(
                operationStatus: LogResultStatusEnum.NonRetriableError,
                resultData: resultData,
                message: operationMessage,
                logMessage: logMessage,
                logData: logData 
            );

        /// <summary>
        /// Performs logic to end processing an operation.  The OperationResult from EndOperation is ready
        /// to be immediately returned.
        /// </summary>
        /// <param name="fromResult">A result from the operationAction code that is used as a 
        /// basis for the operation ending logic. </param>
        /// <param name="operationStatus">A status for the operation to be used if fromResult is null 
        /// </param>
        /// <param name="operationMessage">A message for the operation to be used if fromResult is null
        /// </param>
        /// <param name="resultData">Result data for the operation to be used if fromResult is null
        /// </param>
        /// <param name="logMessage">The message to write to the log.  This will override the
        /// operation message, which is normally written</param>
        /// <param name="logResultData">The result data to write to the log.  This will override 
        /// operation result data, which is normally written</param>
        /// <returns>The result to return from the operation</returns>
        protected OperationResult<TResultData> EndOperation<TResultData>(
            OperationResult<TResultData> fromResult = null,
            LogResultStatusEnum? operationStatus = null,
            string operationMessage = null,
            string operationCode = null,
            TResultData resultData = null,
            string logMessage = null,
            object logResultData = null
        ) where TResultData : class {
            
            LogResultStatusEnum useStatus = 
                operationStatus ?? fromResult?.OperationStatus ??             
                    (Log.IsInErrorState ? LogResultStatusEnum.NonRetriableError : LogResultStatusEnum.Success);
            string useMessage = 
                operationMessage ?? fromResult?.Message ?? 
                    (Log.IsInErrorState ? Log.ErrorMessage : null);
            object useCode = 
                operationCode ?? fromResult?.Code;
            TResultData useResultData = resultData ?? fromResult?.ResultData;

            Log.End(
                useStatus,
                logMessage ?? fromResult?.LogMessage ?? useMessage,
                logResultData ?? fromResult?.LogData ?? useResultData,
                resultCode: useCode
            );

            Log = null;

            isActive = false;

            return Result(
                useStatus,
                useResultData,
                useMessage,
                useCode
            );
        }

        /// <summary>
        /// Performs logic to end processing an operation.  The OperationResult from EndOperation is ready
        /// to be immediately returned.
        /// </summary>
        /// <param name="fromResult">A result from the operationAction code that is used as a 
        /// basis for the operation ending logic. </param>
        /// <param name="operationStatus">A status for the operation to be used if fromResult is null 
        /// </param>
        /// <param name="operationMessage">A message for the operation to be used if fromResult is null
        /// </param>
        /// <param name="resultData">Result data for the operation to be used if fromResult is null
        /// </param>
        /// <param name="logMessage">The message to write to the log.  This will override the
        /// operation message, which is normally written</param>
        /// <param name="logResultData">The result data to write to the log.  This will override 
        /// operation result data, which is normally written</param>
        /// <returns>The result to return from the operation</returns>
        protected OperationResult EndOperation(
            OperationResult fromResult = null,
            LogResultStatusEnum? operationStatus = null,
            string operationMessage = null,
            string operationCode = null,
            string logMessage = null
        ) {
            LogResultStatusEnum useStatus = 
                operationStatus ?? fromResult?.OperationStatus ??             
                    (Log.IsInErrorState ? LogResultStatusEnum.NonRetriableError : LogResultStatusEnum.Success);
            string useMessage = 
                operationMessage ?? fromResult?.Message ?? 
                    (Log.IsInErrorState ? Log.ErrorMessage : null);
            object useCode = 
                operationCode ?? fromResult?.Code;

            Log.End(
                useStatus,
                logMessage ?? useMessage,
                null,
                useCode
            );

            Log = null;

            isActive = false;

            return Result(
                useStatus,
                message: useMessage,
                useCode
            );
        }

        /// <summary>
        /// Creates an operation result for return
        /// </summary>
        /// <param name="operationStatus">status to use</param>
        /// <param name="resultData">result data to use</param>
        /// <param name="operationMessage">result message</param>
        /// <returns>Operation result for immediate return</returns>
        protected static OperationResult<TResultData> Result<TResultData>(
            LogResultStatusEnum? operationStatus,
            TResultData resultData,
            string message = null,
            object code = null,            
            string logMessage = null,
            TResultData logData = null
        ) where TResultData : class => new() {
            Message = message,
            OperationStatus = operationStatus,
            ResultData = resultData,
            LogMessage = logMessage,
            LogData = logData,
            Code = code
        };

        /// <summary>
        /// Creates an operation result for return
        /// </summary>
        /// <param name="operationStatus">status to use</param>
        /// <param name="resultData">result data to use</param>
        /// <param name="operationMessage">result message</param>
        /// <returns>Operation result for immediate return</returns>
        protected static OperationResult Result(
            LogResultStatusEnum? operationStatus,
            string message = null,
            object code = null,            
            string logMessage = null
        ) => new() {
            Message = message,
            OperationStatus = operationStatus,
            LogMessage = logMessage,
            Code = code
        };

        // =====================================================================
        // Monitoring and Log
        // =====================================================================

        protected static object FormatExceptionForLog(Exception ex) 
            => new {
                    ExceptionMessage = ex.Message,
                    ExceptionStack = ex.StackTrace,
                    ExceptionTarget = ex.TargetSite,
                    ExceptionSource = ex.Source
                };

        protected void LogExceptionAsErratum(
            Exception ex,
            string message = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0
        ) {
            Log.ErratumAs(
                message: message ?? ex.Message, 
                attributes: FormatExceptionForLog(ex).FieldsToDictionary(),
                filePath: callerFilePath,
                memberName: callerMemberName,
                lineNumber: callerLineNumber
            );
        }

        protected void LogExceptionAsFatality(
            Exception ex,
            string message = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0
        ) {
            Log.FatalityAs(
                message: message ?? ex.Message, 
                attributes: FormatExceptionForLog(ex).FieldsToDictionary(),
                filePath: callerFilePath,
                memberName: callerMemberName,
                lineNumber: callerLineNumber
            );
        }

        protected void LogExceptionAsError(
            Exception ex,
            string message = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0
        ) {
            Log.ErrorAs(
                message: message ?? ex.Message, 
                attributes: FormatExceptionForLog(ex).FieldsToDictionary(),
                filePath: callerFilePath,
                memberName: callerMemberName,
                lineNumber: callerLineNumber
            );
        }

        protected void LogExceptionAsWarning(
            Exception ex,
            string message = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0
        ) {
            Log.ErrorAs(
                message: message ?? ex.Message, 
                attributes: FormatExceptionForLog(ex).FieldsToDictionary(),
                filePath: callerFilePath,
                memberName: callerMemberName,
                lineNumber: callerLineNumber
            );
        }

        /// <summary>
        /// Log an erratum (bug, unexpected behaviour)
        /// </summary>
        protected void Erratum(
            string message = null,
            IReadOnlyDictionary<string, object> attributes = null,
            IEnumerable<string> tags = null,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = 0
        ) => Log.ErratumAs(
                message,
                attributes,
                tags,
                callerFilePath,
                callerMemberName,
                callerLineNumber
            );

        /// <summary>
        /// Log an erratum (bug, unexpected behaviour)
        /// </summary>
        protected void Erratum(
            string message,
            object attributes,
            IEnumerable<string> tags = null,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = 0
        ) => Log.ErratumAs(
                message,
                attributes?.FieldsToDictionary(),
                tags,
                callerFilePath,
                callerMemberName,
                callerLineNumber
            );

        /// <summary>
        /// Log an error (a plausible error, as opposed to unexpected behaviour). 
        /// </summary>
        protected void Error(
            string message = null,
            IReadOnlyDictionary<string, object> attributes = null,
            object code = null,
            IEnumerable<string> tags = null,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = 0
        ) => Log.ErrorAs(
                message,
                attributes,
                code,
                tags,
                callerFilePath,
                callerMemberName,
                callerLineNumber
            );

        /// <summary>
        /// Log an error (a plausible error, as opposed to unexpected behaviour)
        /// </summary>
        protected void Error(
            string message,
            object attributes,
            object code = null,
            IEnumerable<string> tags = null,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = 0
        ) => Log.ErrorAs(
                message,
                attributes?.FieldsToDictionary(),
                code,
                tags,
                callerFilePath,
                callerMemberName,
                callerLineNumber
            );

        /// <summary>
        /// Log a warning 
        /// </summary>
        protected void Warning(
            string message = null,
            IReadOnlyDictionary<string, object> attributes = null,
            object code = null,
            IEnumerable<string> tags = null,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = 0
        ) => Log.WarningAs(
            message,
            attributes,
            code,
            tags,
            callerFilePath,
            callerMemberName,
            callerLineNumber
        );

        /// <summary>
        /// Log a warning 
        /// </summary>
        protected void Warning(
            string message,
            object attributes,
            object code = null,
            IEnumerable<string> tags = null,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = 0
        ) => Log.WarningAs(
            message,
            attributes?.FieldsToDictionary(),
            code,
            tags,
            callerFilePath,
            callerMemberName,
            callerLineNumber
        );

        /// <summary>
        /// Log a status update 
        /// </summary>
        protected void Status(
            string message,
            IReadOnlyDictionary<string, object> attributes = null,
            object code = null,
            IEnumerable<string> tags = null
        ) => Log.Status(
            message,
            attributes,
            code,
            tags
        );
        
        /// <summary>
        /// Log a status update 
        /// </summary>
        protected void Status(
            string message,
            object attributes,
            object code = null,
            IEnumerable<string> tags = null
        ) => Log.Status(
            message,
            attributes?.FieldsToDictionary(),
            code,
            tags
        );

        /// <summary>
        /// Log a trace message (for debugging and analysis) 
        /// </summary>
        protected void Trace(
            string message,
            IReadOnlyDictionary<string, object> attributes,
            object code = null,
            IEnumerable<string> tags = null,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = 0
        ) => Log.TraceAs(
            message,
            attributes,
            code,
            tags,
            callerFilePath,
            callerMemberName,
            callerLineNumber
        );
                
        /// <summary>
        /// Log a trace message (for debugging and analysis) 
        /// </summary>
        protected void Trace(
            string message,
            object attributes,
            object code,
            IEnumerable<string> tags = null,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = 0
        ) => Log.TraceAs(
            message,
            attributes?.FieldsToDictionary(),
            code,
            tags,
            callerFilePath,
            callerMemberName,
            callerLineNumber
        );

        /// <summary>
        /// Log a trace message (for debugging and analysis) 
        /// </summary>
        protected void Trace(
            string message,
            object attributes,
            IEnumerable<string> tags = null,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = 0
        ) => Log.TraceAs(
            message,
            attributes?.FieldsToDictionary(),
            null,
            tags,
            callerFilePath,
            callerMemberName,
            callerLineNumber
        );

        /// <summary>
        /// Log a trace message (for debugging and analysis) 
        /// </summary>
        protected void Trace(
            object attributes,
            IEnumerable<string> tags = null,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = 0
        ) => Log.TraceAs(
            "",
            attributes?.FieldsToDictionary(),
            null,
            tags,
            callerFilePath,
            callerMemberName,
            callerLineNumber
        );

        /// <summary>
        /// Log a trace message (for debugging and analysis) 
        /// </summary>
        protected void Trace(
            string message,
            IEnumerable<string> tags = null,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = 0
        ) => Log.TraceAs(
            message,
            (Dictionary<string, object>)null,
            null,
            tags,
            callerFilePath,
            callerMemberName,
            callerLineNumber
        );

        /// <summary>
        /// Log a trace message (for debugging and analysis) 
        /// </summary>
        protected void Trace(
            IEnumerable<string> tags = null,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = 0
        ) => Log.TraceAs(
            "",
            (Dictionary<string, object>)null,
            null,
            tags,
            callerFilePath,
            callerMemberName,
            callerLineNumber
        );

        /// <summary>
        /// Log information that is part of an audit trail
        /// </summary>
        protected void Audit(
            string message, 
            object attributes = null, 
            object code = null, 
            IEnumerable<string> tags = null
        ) => Log.Audit(
            message,
            attributes == null ? (IReadOnlyDictionary<string, object>) null : attributes.FieldsToDictionary(),
            code,
            tags            
        );

        /// <summary>
        /// Log information that is part of an audit trail
        /// </summary>
        protected void Audit(
            string message, 
            IReadOnlyDictionary<string, object> attributes = null, 
            object code = null, 
            IEnumerable<string> tags = null
        ) => Log.Audit(
            message,
            attributes,
            code,
            tags            
        );

        /// <summary>
        /// Log info (generally for debugging and analysis.  If otherwise, consider Status instead) 
        /// </summary>
        protected void Info(
            string message = null,
            IReadOnlyDictionary<string, object> attributes = null,
            object data = null,
            object code = null, 
            IEnumerable<string> tags = null
        ) => Log.Info(
            message,
            attributes,
            data,
            code,
            tags
        );
        
        /// <summary>
        /// Log info (generally for debugging and analysis.  If otherwise, consider Status instead) 
        /// </summary>
        protected void Info(
            string message,
            object attributes,
            object data = null,
            object code = null, 
            IEnumerable<string> tags = null
        ) => Log.Info(
            message,
            attributes?.FieldsToDictionary(),
            data,
            code,
            tags
        );

        /// <summary>
        /// Log a snapshot of operation data.  This may be used by downstream processes analyzing the
        /// operation for specific purposes. 
        /// </summary>
        protected void Snapshot(
            IReadOnlyDictionary<string, object> attributes,
            object data = null,
            object code = null,
            IEnumerable<string> tags = null
        ) => Log.Snapshot(
            attributes,
            data,
            code,
            tags
        );

        /// <summary>
        /// Log a snapshot of operation data.  This may be used by downstream processes analyzing the
        /// operation for specific purposes. 
        /// </summary>
        protected void Snapshot(
            object attributes,
            object data = null,
            object code = null,
            IEnumerable<string> tags = null
        ) => Log.Snapshot(
            attributes?.FieldsToDictionary(),
            data,
            code,
            tags
        );

        /// <summary>
        /// Automatically log a value on exit.
        /// </summary>
        protected void AutoLog(
            string name,
            object value
        ) => Log.AutoLog(name, value);

        /// <summary>
        /// Automatically log a value on exit.
        /// </summary>
        protected void AutoLog(
            object values
        ) => Log.AutoLog(values);
    }
}

using System;
using System.Collections.Generic;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Utility;
using XKit.Lib.Common.Utility.Extensions;
using XKit.Lib.Log.Entities;

namespace XKit.Lib.Log {

    internal class LogSession : ILogSession {

        private readonly ILogWriter logWriter;
        private readonly string originatorName;
        private readonly int? originatorVersion; 
        private readonly SetOnceOrThrow<string> originatorFabricId = new(); 
        private readonly string originatorInstanceId; 
        private readonly string correlationId;
        private readonly Stack<LogSessionState> stateStack = new();
        private bool isInErrorState = false;
        private string errorMessage = "";
        private object errorCode = null;

        private LogSessionState State { 
            get { 
                lock (stateStack) {
                    if (stateStack.Count == 0) {
                        return null;
                    }
                    return stateStack.Peek();
                }
            }
        }

        public LogSession(
            ILogWriter logWriter,
            string originatorName, 
            int? originatorVersion, 
            string originatorFabricId, 
            string originatorInstanceId, 
            string correlationId
        ) {
            this.logWriter = logWriter;
            this.originatorName = originatorName;
            this.originatorVersion = originatorVersion;
            if (originatorFabricId != null) {
                this.originatorFabricId.Value = originatorFabricId;
            }
            this.originatorInstanceId = originatorInstanceId;
            this.correlationId = correlationId ?? Common.Utility.Identifiers.GenerateIdentifier();
        }

        bool ILogSession.IsInErrorState => isInErrorState;

        string ILogSession.ErrorMessage => isInErrorState ? errorMessage : "";

        object ILogSession.ErrorCode => errorCode;

        string ILogSession.OriginatorName => originatorName;

        int? ILogSession.OriginatorVersion => originatorVersion;

        string ILogSession.OriginatorFabricId => originatorFabricId;

        string ILogSession.OriginatorInstanceId => originatorInstanceId;

        string ILogSession.CorrelationId => correlationId;

        ILogSession ILogSession.AutoLog(IDictionary<string, object> values) {
            var sessionValues = State?.AutoLogValues;
            if (sessionValues == null) { return this; }

            lock (sessionValues) {
                values?.ForEach(kv => sessionValues[kv.Key] = kv.Value);            
            }
            return this;
        }

        ILogSession ILogSession.AutoLog(string key, object value) {
            var sessionValues = State?.AutoLogValues;
            if (sessionValues == null) { return this; }
            lock (sessionValues) {
                sessionValues[key] = value;            
            }
            return this;
        }

        ILogSession ILogSession.AutoLog(object anonObjectAsKeysAndValues) {
            var sessionValues = State.AutoLogValues;
            if (sessionValues == null) { return this; }
            lock (sessionValues) {
                anonObjectAsKeysAndValues?.FieldsToDictionary().ForEach(kv => sessionValues[kv.Key] = kv.Value);            
            }
            return this;
        }

        void ILogSession.SetFabricId(string fabricId) => originatorFabricId.Value = fabricId;
        
        void ILogSession.Begin(
            LogContextTypeEnum contextType, 
            string operationName, 
            object workItem,
            Guid contextId,
            IReadOnlyDictionary<string, object> attributes
        ) {
            errorMessage = "";
            isInErrorState = false;
            Begin(contextType, operationName, workItem, contextId, attributes);
        }

        void ILogSession.Pending(
            string message,
            object code,
            IReadOnlyDictionary<string, object> attributes
        ) {
            Pending(message, code, attributes);
        }

        void ILogSession.End(
            LogResultStatusEnum status, 
            string message, 
            object resultData, 
            object resultCode
        ) {
            End(status, message, resultData, resultCode);
        }

        void ILogSession.Audit(
            string message, 
            IReadOnlyDictionary<string, object> attributes, 
            object code, 
            IEnumerable<string> tags
        ) {
            WriteEvent(
                eventType: LogEventTypeEnum.Audit,
                message: message,
                attributes: attributes,
                code: code,
                tags: tags
            );
        }

        void ILogSession.Audit(
            string message, 
            object attributes, 
            object code, 
            IEnumerable<string> tags
        ) {
            WriteEvent(
                eventType: LogEventTypeEnum.Audit,
                message: message,
                attributes: attributes?.FieldsToDictionary(),
                code: code,
                tags: tags
            );
        }

        void ILogSession.Erratum(
            string message, 
            IReadOnlyDictionary<string, object> attributes, 
            IEnumerable<string> tags, 
            string callerFilePath, 
            string callerMemberName, 
            int callerLineNumber
        ) {
            WriteEvent(
                eventType: LogEventTypeEnum.Erratum,
                message: message,
                attributes: attributes,
                tags: tags,
                callerFilePath: callerFilePath,
                callerMemberName: callerMemberName,
                callerLineNumber: callerLineNumber
            );
        }

        void ILogSession.Erratum(
            string message, 
            object attributes, 
            IEnumerable<string> tags, 
            string callerFilePath, 
            string callerMemberName, 
            int callerLineNumber
        ) {
            WriteEvent(
                eventType: LogEventTypeEnum.Erratum,
                message: message,
                attributes: attributes?.FieldsToDictionary(),
                tags: tags,
                callerFilePath: callerFilePath,
                callerMemberName: callerMemberName,
                callerLineNumber: callerLineNumber
            );
        }

        void ILogSession.ErratumAs(
            string message, 
            IReadOnlyDictionary<string, object> attributes, 
            IEnumerable<string> tags, 
            string filePath, 
            string memberName, 
            int lineNumber
        ) {
            WriteEvent(
                eventType: LogEventTypeEnum.Erratum,
                message: message,
                attributes: attributes,
                tags: tags,
                callerFilePath: filePath,
                callerMemberName: memberName,
                callerLineNumber: lineNumber
            );
        }

        void ILogSession.Error(
            string message, 
            IReadOnlyDictionary<string, object> attributes, 
            object code, 
            IEnumerable<string> tags, 
            string callerFilePath, 
            string callerMemberName, 
            int callerLineNumber
        ) {
            errorMessage = message;
            isInErrorState = true;
            errorCode = code;
            WriteEvent(
                eventType: LogEventTypeEnum.Error,
                message: message,
                attributes: attributes,
                tags: tags,
                code: code,
                callerFilePath: callerFilePath,
                callerMemberName: callerMemberName,
                callerLineNumber: callerLineNumber
            );
        }

        void ILogSession.Error(
            string message, 
            object attributes, 
            object code, 
            IEnumerable<string> tags, 
            string callerFilePath, 
            string callerMemberName, 
            int callerLineNumber
        ) {
            errorMessage = message;
            errorCode = code;
            isInErrorState = true;
            WriteEvent(
                eventType: LogEventTypeEnum.Error,
                message: message,
                attributes: attributes?.FieldsToDictionary(),
                tags: tags,
                code: code,
                callerFilePath: callerFilePath,
                callerMemberName: callerMemberName,
                callerLineNumber: callerLineNumber
            );
        }

        void ILogSession.ErrorAs(
            string message, 
            IReadOnlyDictionary<string, object> attributes, 
            object code, 
            IEnumerable<string> tags, 
            string filePath, 
            string memberName, 
            int lineNumber
        ) {
            errorMessage = message;
            errorCode = code;
            isInErrorState = true;
            WriteEvent(
                eventType: LogEventTypeEnum.Error,
                message: message,
                attributes: attributes,
                tags: tags,
                code: code,
                callerFilePath: filePath,
                callerMemberName: memberName,
                callerLineNumber: lineNumber
            );
        }

        void ILogSession.Fatality(
            string message, 
            IReadOnlyDictionary<string, object> attributes, 
            IEnumerable<string> tags, 
            string callerFilePath, 
            string callerMemberName, 
            int callerLineNumber
        ) {
            errorMessage ??= message;
            isInErrorState = true;
            WriteEvent(
                eventType: LogEventTypeEnum.Fatality,
                message: message,
                attributes: attributes,
                tags: tags,
                callerFilePath: callerFilePath,
                callerMemberName: callerMemberName,
                callerLineNumber: callerLineNumber
            );
        }

        void ILogSession.Fatality(
            string message, 
            object attributes, 
            IEnumerable<string> tags, 
            string callerFilePath, 
            string callerMemberName, 
            int callerLineNumber
        ) {
            errorMessage ??= message;
            isInErrorState = true;
            WriteEvent(
                eventType: LogEventTypeEnum.Fatality,
                message: message,
                attributes: attributes?.FieldsToDictionary(),
                tags: tags,
                callerFilePath: callerFilePath,
                callerMemberName: callerMemberName,
                callerLineNumber: callerLineNumber
            );
        }

        void ILogSession.FatalityAs(
            string message, 
            IReadOnlyDictionary<string, object> attributes, 
            IEnumerable<string> tags, 
            string callerFilePath, 
            string callerMemberName, 
            int callerLineNumber
        ) {
            errorMessage ??= message;
            isInErrorState = true;
            WriteEvent(
                eventType: LogEventTypeEnum.Fatality,
                message: message,
                attributes: attributes,
                tags: tags,
                callerFilePath: callerFilePath,
                callerMemberName: callerMemberName,
                callerLineNumber: callerLineNumber
            );
        }

        void ILogSession.Info(
            string message, 
            IReadOnlyDictionary<string, object> attributes, 
            object data, 
            object code, 
            IEnumerable<string> tags
        ) {
            WriteEvent(
                eventType: LogEventTypeEnum.Info,
                message: message,
                attributes: attributes,
                tags: tags,
                code: code,
                data: data
            );
        }

        void ILogSession.Info(
            string message, 
            object attributes, 
            object data, 
            object code, 
            IEnumerable<string> tags
        ) {
            WriteEvent(
                eventType: LogEventTypeEnum.Info,
                message: message,
                attributes: attributes?.FieldsToDictionary(),
                tags: tags,
                code: code,
                data: data
            );
        }

        void ILogSession.NewEvent(
            LogEventTypeEnum eventType, 
            string message, 
            object data, 
            IReadOnlyDictionary<string, object> attributes, 
            object code, 
            IEnumerable<string> tags,
            string callerFilePath, 
            string callerMemberName, 
            int callerLineNumber
        ) {
            if (eventType == LogEventTypeEnum.Error) {
                errorMessage = message;
                errorCode = code;
                isInErrorState = true;                
            }
            WriteEvent(
                eventType: eventType,
                message: message,
                attributes: attributes,
                tags: tags,
                code: code,
                data: data,
                callerFilePath: callerFilePath,
                callerMemberName: callerMemberName,
                callerLineNumber: callerLineNumber
            );
        }

        void ILogSession.NewEvent(
            string eventTypeName, 
            string message, 
            object data, 
            IReadOnlyDictionary<string, object> attributes, 
            object code, 
            IEnumerable<string> tags,
            string callerFilePath, 
            string callerMemberName, 
            int callerLineNumber
        ) {
            if (eventTypeName == LogEventTypeEnum.Error.ToString()) {
                errorMessage = message;
                errorCode = code;
                isInErrorState = true;                
            }
            WriteEvent(
                eventTypeName: eventTypeName,
                message: message,
                attributes: attributes,
                tags: tags,
                code: code,
                data: data,
                callerFilePath: callerFilePath,
                callerMemberName: callerMemberName,
                callerLineNumber: callerLineNumber
            );
        }

        void ILogSession.Snapshot(
            IReadOnlyDictionary<string, object> attributes, 
            object data, 
            object code, 
            IEnumerable<string> tags
        ) {
            WriteEvent(
                eventType: LogEventTypeEnum.Snapshot,
                attributes: attributes,
                tags: tags,
                code: code,
                data: data
            );
        }

        void ILogSession.Status(
            string message, 
            IReadOnlyDictionary<string, object> attributes, 
            object code, 
            IEnumerable<string> tags
        ) {
            WriteEvent(
                eventType: LogEventTypeEnum.Status,
                message: message,
                attributes: attributes,
                tags: tags,
                code: code
            );
        }

        void ILogSession.Status(
            string message, 
            object attributes, 
            object code, 
            IEnumerable<string> tags
        ) {
            WriteEvent(
                eventType: LogEventTypeEnum.Status,
                message: message,
                attributes: attributes?.FieldsToDictionary(),
                tags: tags,
                code: code
            );
        }

        void ILogSession.Trace(
            string message, 
            IReadOnlyDictionary<string, object> attributes, 
            object code, 
            IEnumerable<string> tags, 
            string callerFilePath, 
            string callerMemberName, 
            int callerLineNumber
        ) {
            WriteEvent(
                eventType: LogEventTypeEnum.Trace,
                message: message,
                attributes: attributes,
                tags: tags,
                code: code,
                callerFilePath: callerFilePath,
                callerMemberName: callerMemberName,
                callerLineNumber: callerLineNumber
            );
        }

        void ILogSession.Trace(
            string message, 
            object attributes, 
            object code, 
            IEnumerable<string> tags, 
            string callerFilePath, 
            string callerMemberName, 
            int callerLineNumber
        ) {
            WriteEvent(
                eventType: LogEventTypeEnum.Trace,
                message: message,
                attributes: attributes?.FieldsToDictionary(),
                tags: tags,
                code: code,
                callerFilePath: callerFilePath,
                callerMemberName: callerMemberName,
                callerLineNumber: callerLineNumber
            );
        }

        void ILogSession.TraceAs(
            string message, 
            IReadOnlyDictionary<string, object> attributes, 
            object code, 
            IEnumerable<string> tags, 
            string filePath, 
            string memberName, 
            int lineNumber
        ) {
            WriteEvent(
                eventType: LogEventTypeEnum.Trace,
                message: message,
                attributes: attributes,
                tags: tags,
                code: code,
                callerFilePath: filePath,
                callerMemberName: memberName,
                callerLineNumber: lineNumber
            );
        }

        void ILogSession.Warning(
            string message, 
            IReadOnlyDictionary<string, object> attributes, 
            object code, 
            IEnumerable<string> tags, 
            string callerFilePath, 
            string callerMemberName, 
            int callerLineNumber
        ) {
            WriteEvent(
                eventType: LogEventTypeEnum.Warning,
                message: message,
                attributes: attributes,
                tags: tags,
                code: code,
                callerFilePath: callerFilePath,
                callerMemberName: callerMemberName,
                callerLineNumber: callerLineNumber
            );
        }

        void ILogSession.Warning(
            string message, 
            object attributes, 
            object code, 
            IEnumerable<string> tags, 
            string callerFilePath, 
            string callerMemberName, 
            int callerLineNumber
        ) {
            WriteEvent(
                eventType: LogEventTypeEnum.Warning,
                message: message,
                attributes: attributes?.FieldsToDictionary(),
                tags: tags,
                code: code,
                callerFilePath: callerFilePath,
                callerMemberName: callerMemberName,
                callerLineNumber: callerLineNumber
            );
        }

        void ILogSession.WarningAs(
            string message, 
            IReadOnlyDictionary<string, object> attributes, 
            object code, 
            IEnumerable<string> tags, 
            string filePath, 
            string memberName, 
            int lineNumber
        ) {
            WriteEvent(
                eventType: LogEventTypeEnum.Warning,
                message: message,
                attributes: attributes,
                tags: tags,
                code: code,
                callerFilePath: filePath,
                callerMemberName: memberName,
                callerLineNumber: lineNumber
            );
        }

        void Begin(
            LogContextTypeEnum contextType, 
            string operationName, 
            object workItem,
            Guid contextId,
            IReadOnlyDictionary<string, object> attributes
        ) {
            lock (stateStack) {
                stateStack.Push(new LogSessionState {
                    ContextId = contextId != default ? contextId : Guid.NewGuid(),
                    ContextType = contextType,
                    OperationName = operationName
                });

                WriteEvent(
                    eventType: stateStack.Count == 0 ? LogEventTypeEnum.Enter : LogEventTypeEnum.ContextChange,
                    data: workItem,
                    attributes: attributes
                );
            }
        }

        void Pending(
            string message = "", 
            object code = null,
            IReadOnlyDictionary<string, object> attributes = null
        ) {
            WriteEvent(
                LogEventTypeEnum.Pending,
                message: message,
                code: code,
                attributes: attributes
            );
        }

        void End(
            LogResultStatusEnum status = LogResultStatusEnum.Unknown, 
            string message = "", 
            object resultData = null, 
            object resultCode = null
        ) {
            lock (stateStack) {
                WriteEvent(
                    stateStack.Count == 1 ? LogEventTypeEnum.Exit : LogEventTypeEnum.ContextChange,
                    message: message,
                    data: resultData,
                    code: resultCode,
                    attributes: State?.AutoLogValues,
                    contextResultStatus: status
                );
                if (stateStack.Count == 0) {
                    // in case we have a bug and are off by one, do not fail.
                    WriteEvent(
                        eventType: LogEventTypeEnum.Erratum,
                        message: "Log Begin/End session mismatch!"
                    );
                    return;
                }
                stateStack.Pop();
            }
        }

        void WriteEvent(
            LogEventTypeEnum? eventType = null,
            string eventTypeName = null,
            string message = null,
            object data = null,
            IReadOnlyDictionary<string, object> attributes = null,
            object code = null,
            IEnumerable<string> tags = null,
            LogResultStatusEnum? contextResultStatus = null,
            string callerFilePath = null, 
            string callerMemberName = null, 
            int? callerLineNumber = null
        ) {
            if (logWriter == null) { return; }

            var e = new LogEventEntry {
                OriginatorFabricId = this.originatorFabricId,
                OriginatorInstanceId = this.originatorInstanceId,
                OriginatorName = this.originatorName,
                OriginatorVersion = this.originatorVersion,
                CorrelationId = this.correlationId,
                ContextType = State?.ContextType ?? LogContextTypeEnum.OtherUnknown,
                ContextId = State?.ContextId ?? Guid.Empty,
                OperationName = State?.OperationName ?? "[Unknown state]",
                EventType = eventType,
                EventTypeName = eventTypeName,
                Message = message,
                Data = data,
                Attributes = attributes,
                Code = code,
                Tags = tags,
                ContextResultStatus = contextResultStatus,
                CallerFilePath = callerFilePath,
                CallerMemberName = callerMemberName,
                CallerLineNumber = callerLineNumber
            };

            logWriter.WriteEvent(e);
        }        
    }
}

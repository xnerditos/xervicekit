using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace XKit.Lib.Common.Log {

    public interface ILogSession {
        bool IsInErrorState { get; }
        string ErrorMessage { get; }
        string OriginatorName { get; }
        int? OriginatorVersion { get; }
        string OriginatorFabricId { get; }
        string OriginatorInstanceId { get; }
        string CorrelationId { get; }

        // ---------------------------------------------------------------------

        void SetFabricId(string fabricId);

        void Begin(
            LogContextTypeEnum contextType,
            string operationName = null,
            object workItem = null,
            Guid contextId = default,
            IReadOnlyDictionary<string, object> attributes = null
        );

        void End(
            LogResultStatusEnum status,
            string message = null,
            object resultData = null,
            object resultCode = null
        );

        void Pending(
            string message = null,
            object code = null,
            IReadOnlyDictionary<string, object> attributes = null
        );

        // ---------------------------------------------------------------------

        void NewEvent(
            LogEventTypeEnum eventType,
            string message = null,
            object data = null,
            IReadOnlyDictionary<string, object> attributes = null,
            object code = null,
            IEnumerable<string> tags = null,
            string callerFilePath = null,
            string callerMemberName = null,
            int callerLineNumber = 0
        );

        void NewEvent(
            string eventTypeName,
            string message = null,
            object data = null,
            IReadOnlyDictionary<string, object> attributes = null,
            object code = null,
            IEnumerable<string> tags = null,
            string callerFilePath = null,
            string callerMemberName = null,
            int callerLineNumber = 0
        );

        // ---------------------------------------------------------------------

        void Fatality(
            string message,
            IReadOnlyDictionary<string, object> attributes,
            IEnumerable<string> tags = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0
        );

        void Fatality(
            string message = null,
            object attributes = null,
            IEnumerable<string> tags = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0
        );

        void FatalityAs(
            string message = null,
            IReadOnlyDictionary<string, object> attributes = null,
            IEnumerable<string> tags = null,
            string filePath = "",
            string memberName = "",
            int lineNumber = 0
        );

        // ---------------------------------------------------------------------

        void Erratum(
            string message,
            IReadOnlyDictionary<string, object> attributes,
            IEnumerable<string> tags = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0
        );

        void Erratum(
            string message = null,
            object attributes = null,
            IEnumerable<string> tags = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0
        );

        void ErratumAs(
            string message = null,
            IReadOnlyDictionary<string, object> attributes = null,
            IEnumerable<string> tags = null,
            string filePath = "",
            string memberName = "",
            int lineNumber = 0
        );

        // ---------------------------------------------------------------------

        void Error(
            string message,
            IReadOnlyDictionary<string, object> attributes,
            object code = null,
            IEnumerable<string> tags = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0
        );

        void Error(
            string message = null,
            object attributes = null,
            object code = null,
            IEnumerable<string> tags = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0
        );

        void ErrorAs(
            string message = null,
            IReadOnlyDictionary<string, object> attributes = null,
            object code = null,
            IEnumerable<string> tags = null,
            string filePath = "",
            string memberName = "",
            int lineNumber = 0
        );

        // ---------------------------------------------------------------------

        void Warning(
            string message,
            IReadOnlyDictionary<string, object> attributes,
            object code = null,
            IEnumerable<string> tags = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0
        );

        void Warning(
            string message = null,
            object attributes = null,
            object code = null,
            IEnumerable<string> tags = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0
        );

        void WarningAs(
            string message = null,
            IReadOnlyDictionary<string, object> attributes = null,
            object code = null,
            IEnumerable<string> tags = null,
            string filePath = "",
            string memberName = "",
            int lineNumber = 0
        );

        // ---------------------------------------------------------------------

        void Status(
            string message,
            IReadOnlyDictionary<string, object> attributes,
            object code = null,
            IEnumerable<string> tags = null
        );

        void Status(
            string message = null,
            object attributes = null,
            object code = null,
            IEnumerable<string> tags = null
        );

        // ---------------------------------------------------------------------

        void Trace(
            string message,
            IReadOnlyDictionary<string, object> attributes,
            object code = null,
            IEnumerable<string> tags = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0
        );

        void Trace(
            string message = null,
            object attributes = null,
            object code = null,
            IEnumerable<string> tags = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = 0
        );

        void TraceAs(
            string message = null,
            IReadOnlyDictionary<string, object> attributes = null,
            object code = null,
            IEnumerable<string> tags = null,
            string filePath = "",
            string memberName = "",
            int lineNumber = 0
        );

        // ---------------------------------------------------------------------

        void Audit(
            string message,
            IReadOnlyDictionary<string, object> attributes,
            object code = null,
            IEnumerable<string> tags = null
        );

        void Audit(
            string message = null,
            object attributes = null,
            object code = null,
            IEnumerable<string> tags = null
        );

        // ---------------------------------------------------------------------

        void Info(
            string message,
            IReadOnlyDictionary<string, object> attributes,
            object data = null,
            object code = null,
            IEnumerable<string> tags = null
        );

        void Info(
            string message = null,
            object attributes = null,
            object data = null,
            object code = null,
            IEnumerable<string> tags = null
        );

        // ---------------------------------------------------------------------

        void Snapshot(
            IReadOnlyDictionary<string, object> attributes,
            object data = null,
            object code = null,
            IEnumerable<string> tags = null
        );

        // ---------------------------------------------------------------------

        ILogSession AutoLog(IDictionary<string, object> values);
        ILogSession AutoLog(string key, object value);
        ILogSession AutoLog(object anonObjectAsKeysAndValues);

    }
}

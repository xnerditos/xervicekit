using System;
using System.Collections.Generic;

namespace XKit.Lib.Common.Log {

    public interface IReadOnlyLogEventEntry {
        LogEventTypeEnum? EventType { get; }
        string EventTypeName { get; }
        string OriginatorName { get; } 
        int? OriginatorVersion { get; } 
        string OriginatorFabricId { get; } 
        string OriginatorInstanceId { get; } 
        string CorrelationId { get; }
        LogContextTypeEnum? ContextType { get; }
        Guid ContextId { get; }
        string OperationName { get; }
        //object WorkItem { get; }
        string Message { get; }
        object Data { get; }
        IReadOnlyDictionary<string, object> Attributes { get; }
        object Code { get; }
        IEnumerable<string> Tags { get; }
        LogResultStatusEnum? ContextResultStatus { get; }
        string CallerFilePath { get; } 
        string CallerMemberName { get; } 
        int? CallerLineNumber { get; }
    }
}

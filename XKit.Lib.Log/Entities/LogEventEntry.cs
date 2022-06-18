using System;
using System.Collections.Generic;
using XKit.Lib.Common.Log;

namespace XKit.Lib.Log.Entities {

    public class LogEventEntry : IReadOnlyLogEventEntry {
        private DateTime timestamp = DateTime.UtcNow;
        public DateTime Timestamp { 
            get => timestamp; 
            set => timestamp = value.ToUniversalTime();
        }

        public void SetEventType(LogEventTypeEnum value) {
            switch(value) {
                case LogEventTypeEnum.ServiceDefined:
                    return;		// ignore 
                case LogEventTypeEnum.None:
                    EventTypeName = string.Empty;
                    return;
                default:
                    EventTypeName = value.ToString();
                    return;
            }
        }

        public string EventTypeName { get; set; }
        public object Code { get; set; }
        public string Message { get; set; }

        public LogEventTypeEnum? EventType {
            get {
                switch(EventTypeName) {
                case null:
                case "": 
                    return LogEventTypeEnum.None;
                default:
                    LogEventTypeEnum eventType;
                    if (Enum.TryParse<LogEventTypeEnum>(EventTypeName, out eventType)) {
                        return eventType;
                    }
                    return LogEventTypeEnum.ServiceDefined;
                }
            }
            set {
                switch(value) {
                case LogEventTypeEnum.ServiceDefined:
                    return;		// ignore 
                case LogEventTypeEnum.None:
                    EventTypeName = string.Empty;
                    return;
                default:
                    EventTypeName = value.ToString();
                    return;
                }
            }
        }

        public string OriginatorName { get; set; }

        public int? OriginatorVersion { get; set; }

        public string OriginatorFabricId { get; set; }

        public string OriginatorInstanceId { get; set; }

        public string CorrelationId { get; set; }

        public LogContextTypeEnum? ContextType { get; set; }

        public Guid ContextId { get; set; }

        public string OperationName { get; set; }

        //public object WorkItem { get; set; }

        public object Data { get; set; }

        public IReadOnlyDictionary<string, object> Attributes { get; set; }

        public IEnumerable<string> Tags { get; set; }

        public LogResultStatusEnum? ContextResultStatus { get; set; }

        public string CallerFilePath { get; set; }

        public string CallerMemberName { get; set; }

        public int? CallerLineNumber { get; set; }
    }
}

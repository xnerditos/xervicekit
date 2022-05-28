using System;
using System.Collections.Generic;
using XKit.Lib.Common.Log;

namespace XKit.Lib.LocalLog.Entities {
    class LogSessionState {
        
        public Dictionary<string, object> AutoLogValues { get; } = new Dictionary<string, object>();
        public LogContextTypeEnum ContextType { get; init; } 
        public Guid ContextId { get; init; }
        public string OperationName { get; init; }
    }
}

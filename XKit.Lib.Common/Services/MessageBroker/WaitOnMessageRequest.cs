
using System;

namespace XKit.Lib.Common.Services.MessageBroker {
    public class WaitOnMessageRequest {
        public Guid MessageId { get; set; }
        public float? WaitTimeoutSeconds { get; set; }
    }
}
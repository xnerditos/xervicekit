
using System;
using XKit.Lib.Common.Fabric;

namespace XKit.Lib.Common.Services.MessageBroker {
    public class WaitOnMessageResponse {
        public Guid MessageId { get; set; }
        public ServiceCallResult[] Results { get; set; }
        public bool Complete { get; set; }
    }
}
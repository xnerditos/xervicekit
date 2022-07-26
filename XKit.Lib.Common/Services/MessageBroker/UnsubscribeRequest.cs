
using XKit.Lib.Common.Services.MessageBroker;

namespace XKit.Lib.Common.Services.MessageBroker {
    public class UnsubscribeRequest {
        public Subscription[] Subscriptions { get; set; }
    }
}

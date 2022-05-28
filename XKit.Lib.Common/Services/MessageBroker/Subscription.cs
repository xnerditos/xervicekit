using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Client;
using System.Collections.Generic;

namespace XKit.Lib.Common.Services.MessageBroker {

    public interface IReadOnlySubscription {
        string MessageTypeName { get; }
        IReadOnlyDescriptor Recipient { get; }
        string RecipientHostId { get; }
        IReadOnlyServiceCallPolicy Policy { get; }
        ServiceClientErrorHandling? ErrorHandling { get; }
        int? MaxDeliveryRetries { get; }
        int? MaxConsecutiveFailures { get; }
        IReadOnlyList<int> FailureDelaysToRetryMs { get; }
    }

    public class Subscription : IReadOnlySubscription {
        public string MessageTypeName { get; set; }
        public Descriptor Recipient { get; set; }
        public string RecipientHostId { get; set; }
        public ServiceCallPolicy Policy { get; set; }
        public ServiceClientErrorHandling? ErrorHandling { get; set; }
        public int? MaxDeliveryRetries { get; set; }
        public int? MaxConsecutiveFailures { get; set; }
        public int[] FailureDelaysToRetryMs { get; set; }

        IReadOnlyDescriptor IReadOnlySubscription.Recipient => (IReadOnlyDescriptor)this.Recipient;
        IReadOnlyList<int> IReadOnlySubscription.FailureDelaysToRetryMs => this.FailureDelaysToRetryMs;
        IReadOnlyServiceCallPolicy IReadOnlySubscription.Policy => this.Policy;
    }
}
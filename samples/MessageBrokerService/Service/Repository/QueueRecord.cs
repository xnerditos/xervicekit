
using System;
using XKit.Lib.Common.Services.MessageBroker;

namespace Samples.MessageBroker.Engine; 
public interface IQueueRecord {

    Guid QueueId { get; }
    string QueueName { get; }
    IReadOnlySubscription Subscription { get; }
    int ConsecutiveFailureCount { get; }
    QueueState State { get; set; }
}

public class QueueRecord : IQueueRecord {

    public Guid QueueId { get; set; }
    public string QueueName { get; set; }
    public Subscription Subscription { get; set; }
    public int ConsecutiveFailureCount { get; set; }
    public QueueState State { get; set; }

    IReadOnlySubscription IQueueRecord.Subscription => (IReadOnlySubscription)this.Subscription;
}


using System;
using XKit.Lib.Common.Fabric;

namespace Samples.MessageBroker.Engine; 
public interface IQueueItemRecord {
    Guid QueueItemId { get; }
    DateTime SubmittedTimestamp { get; }
    DateTime? StartProcessingTimestamp { get; }
    DateTime? FinishProcessingTimestamp { get; }
    string QueueName { get; }
    Guid MessageId { get; }
    string MessageTypeName { get; }
    DateTime? ReadyInQueue { get; }
    int PreviousAttempts { get; }
    ServiceCallResult[] Results { get; }
    QueueItemState State { get; }
}

public class QueueItemRecord : IQueueItemRecord {
    public Guid QueueItemId { get; set; }
    public DateTime SubmittedTimestamp { get; set; }
    public DateTime? StartProcessingTimestamp { get; set; }
    public DateTime? FinishProcessingTimestamp { get; set; }
    public string QueueName { get; set; }
    public Guid MessageId { get; set; }
    public string MessageTypeName { get; set; }
    public DateTime? ReadyInQueue { get; set; }
    public int PreviousAttempts { get; set; }
    public ServiceCallResult[] Results { get; set; }
    public QueueItemState State { get; set; }
}

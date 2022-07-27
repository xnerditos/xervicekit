
using System;
using System.Collections.Generic;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Services.MessageBroker;

namespace Samples.MessageBroker.Engine; 
public interface IMessageRecord {
    FabricMessage FabricMessage { get; }
    DateTime? StartTimestamp { get; }
    DateTime? FinishTimestamp { get; }
    MessageKind Kind { get; }
    IReadOnlyList<ServiceCallResult> Results { get; }
    Guid LocalOperationId { get; }
}

public class MessageRecord : IMessageRecord {
    public FabricMessage FabricMessage { get; set; }
    public DateTime? StartTimestamp { get; set; }
    public DateTime? FinishTimestamp { get; set; }
    public MessageKind Kind { get; set; }
    public ServiceCallResult[] Results { get; set; }
    public Guid LocalOperationId { get; set; }

    IReadOnlyList<ServiceCallResult> IMessageRecord.Results => Results;
}

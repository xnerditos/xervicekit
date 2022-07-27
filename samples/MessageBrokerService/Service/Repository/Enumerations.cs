namespace Samples.MessageBroker.Engine; 
public enum MessageKind {
    Command,
    Event
}

public enum QueueState {
    Active,
    Inactive,
    Abandoned
}

public enum QueueItemState {
    Enqueued,
    Processing,
    Delivered,
    Dead
}

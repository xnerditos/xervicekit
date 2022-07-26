using System;
using System.Collections.Generic;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Services.MessageBroker;

namespace Samples.MessageBroker.Engine; 
public interface IMessageEngine {

    void SetParameters(MessageEngineParameters p);

    // ---------------------------------------------------------------------
    // Queues
    // ---------------------------------------------------------------------

    // Ensures that a queue exists for each subscription.  
    void EnsureLoadQueues(IReadOnlyList<IReadOnlySubscription> subscriptions);

    // Ensures that a queue exists for the subscription.  If it does, then it
    // is reactivated (in the case of a deactivated queue), the associated
    // subscription info is updated, and any failure counts are reset.
    void CreateRenewSubscription(IReadOnlySubscription subscription);

    // Returns all non-abandoned queues
    IReadOnlyList<IQueueRecord> GetQueues();

    // Returns the queue record for the given name
    IQueueRecord GetQueue(string queueName);

    // Marks a queue as inactive.  Inactive queues continue to accumulate messages,
    // but no delivery attempts are made.
    void DeactivateQueue(string queueName);

    // Marks a queue as abandoned.  Abandoned queues continue to delivery existing 
    // messages, but no new messages will be added.
    void AbandonQueue(string queueName);

    // ---------------------------------------------------------------------
    // Messages
    // ---------------------------------------------------------------------

    // Adds a message to the corresponding queues
    IReadOnlyList<Guid> AddMessage(IMessageRecord record);

    // Gets a message (fully hydrated with results)
    IMessageRecord GetMessage(Guid messageId);

    // Returns true if the message has been delivered.  Note that if a message is
    // in an inactive queue, it may never be delivered until the queue is flushed.
    bool IsMessageDelivered(Guid messageId);

    // ---------------------------------------------------------------------
    // Item processing
    // ---------------------------------------------------------------------

    // Gets the number of queue items that are ready and could be processed. 
    int GetReadyCount();

    // Locks a queue item as "being processed" and returns it.  The queue item
    // returned is the next in a fair order for processing.
    (IQueueItemRecord item, IReadOnlySubscription subscription, string jsonPayload) LockReadyQueueItem(Guid queueItemId = default);

    // The queue item id's returned is the next in a fair order for processing.
    IReadOnlyList<Guid> GetReadyQueueItemIds();

    // Releases a previously locked queue item and returns it to it's previous state
    // without any updates.
    void ReleaseQueueItem(Guid queueItemId);

    // Marks a queue item as "complete".
    void CompleteQueueItemAsDelivered(
        Guid queueItemId, 
        IReadOnlyList<ServiceCallResult> results
    );

    IQueueItemRecord FailQueueItem(
        Guid queueItemId, 
        IReadOnlyList<ServiceCallResult> results 
    );

    // Marks a queue item as "dead".  Although it never succeeded, delivery attempts
    // will no longer be made.
    void KillQueueItem(Guid queueItemId, IReadOnlyList<ServiceCallResult> results);

    // ---------------------------------------------------------------------
    // Other Item 
    // ---------------------------------------------------------------------

    // Gets all of the queue items associated with a message
    IReadOnlyList<IQueueItemRecord> GetQueueItemsForMessage(Guid messageId);

    // Deletes queue items that are in a particular state and older than the given date
    // Note that only items that are not scheduled can be deleted.
    void DeleteQueueItems(DateTime olderThan, QueueItemState state);
}

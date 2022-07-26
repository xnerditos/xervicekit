using System;
using System.Collections.Generic;
using XKit.Lib.Common.Services.MessageBroker;
using XKit.Lib.Common.Utility.Extensions;
using Mapster;
using System.Linq;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Utility;
using XKit.Lib.Common.Client;
using XKit.Lib.Common.Utility.Collections;

namespace Samples.MessageBroker.Engine; 

public class MessageEngine : IMessageEngine {

    public enum LogCodes {
        QueueGoingInactive,
        QueueItemKilled
    }

    private MessageEngineParameters parameters = new() {
        DefaultDeliveryFailureDelaysToRetryMs = new[] { 1000 },
        DefaultMaxConsecutiveFailuresPerQueue = 8,
        DefaultMaxItemRetries = 9
    };

    private readonly SynchronizedList<QueueRecord> queues = new();
    private readonly SynchronizedList<QueueItemRecord> queueItems = new();
    private readonly SynchronizedList<MessageRecord> messageRecords = new();

    public MessageEngine() { } 

    // ---------------------------------------------------------------------
    // Queues
    // ---------------------------------------------------------------------

    void IMessageEngine.EnsureLoadQueues(IReadOnlyList<IReadOnlySubscription> subscriptions) {
        var queueNames = 
            subscriptions.Select(s => s.GetQueueName())
            .ToArray();
        var existingQueueRecords = 
            queues
            .Where(qr => queueNames.Contains(qr.QueueName))
            .Select(qr => qr.QueueName)
            .ToHashSet();
        var newQueueRecords =
            from s in subscriptions
            let queueName = s.GetQueueName()
            where !existingQueueRecords.Contains(queueName)
            select new QueueRecord {
                QueueName = queueName,
                QueueId = Guid.NewGuid(),
                Subscription = s.Adapt<Subscription>(),
                ConsecutiveFailureCount = 0,
                State = QueueState.Active
            };
        newQueueRecords.ForEach(n => queues.Add(n));
    }

    void IMessageEngine.CreateRenewSubscription(IReadOnlySubscription subscription) {
        var queueName = subscription.GetQueueName();
        var existingQueueRecord = 
            queues
            .Where(qr => qr.QueueName == queueName)
            .FirstOrDefault();
        if (existingQueueRecord != null) {
            existingQueueRecord.ConsecutiveFailureCount = 0;
            existingQueueRecord.State = QueueState.Active;

            // Reschedule all items
            var items = queueItems
                .Where(qir => qir.State == QueueItemState.Enqueued)
                .ToArray();
            items.ForEach(qir => qir.ReadyInQueue = qir.SubmittedTimestamp);

        } else {
            var q = new QueueRecord {
                QueueId = Guid.NewGuid(),
                QueueName = queueName,
                State = QueueState.Active,
                ConsecutiveFailureCount = 0,
                Subscription = subscription.Adapt<Subscription>()
            };
            queues.Add(q);
        }
    }

    IReadOnlyList<IQueueRecord> IMessageEngine.GetQueues() {
        return 
            queues
            .Where(qr => qr.State != QueueState.Abandoned)
            .ToArray();
    }

    IQueueRecord IMessageEngine.GetQueue(string queueName) {
        return 
            queues
            .Where(qr => qr.QueueName == queueName)
            .FirstOrDefault();
    }

    void IMessageEngine.DeactivateQueue(string queueName) {
        var queue = 
            queues
            .Where(q => q.QueueName == queueName)
            .SingleOrDefault();
        if (queue == null) {
            return;
        }

        if (queue.State == QueueState.Active) {

            queue.State = QueueState.Inactive;

            // Unschedule all items
            var items = queueItems
                .Where(qir => qir.ReadyInQueue != null)
                .ToArray();
            items.ForEach(qir => qir.ReadyInQueue = null);
        }
    }

    void IMessageEngine.AbandonQueue(string queueName) {
        var queue = 
            queues
            .Where(q => q.QueueName == queueName)
            .SingleOrDefault();
        if (queue == null) {
            return;
        }
        queue.State = QueueState.Abandoned;

        // Existing scheduled items may still be processed
    }

    // ---------------------------------------------------------------------
    // Messages
    // ---------------------------------------------------------------------

    IReadOnlyList<Guid> IMessageEngine.AddMessage(IMessageRecord message) {
        Guid[] queueItemIds;
    
        var messageRecord = message.Adapt<MessageRecord>();
        messageRecord.Results = null;           // don't save any results, these are calculated
        messageRecords.Add(messageRecord);
        
        if (!messageRecord.StartTimestamp.HasValue) {
            messageRecord.StartTimestamp = DateTime.UtcNow;
        }
        
        var queues = 
            this.queues
            .Where(qr => qr.Subscription.MessageTypeName == messageRecord.FabricMessage.MessageTypeName)
            .Where(qr => qr.State != QueueState.Abandoned)
            .ToArray();

        var queueItems =
            queues.Select(
                qr => new QueueItemRecord {
                    QueueItemId = Guid.NewGuid(),
                    MessageId = messageRecord.FabricMessage.MessageId,
                    MessageTypeName = messageRecord.FabricMessage.MessageTypeName,
                    QueueName = qr.QueueName,
                    SubmittedTimestamp = messageRecord.StartTimestamp.Value,
                    PreviousAttempts = 0,
                    State = QueueItemState.Enqueued,
                    // if queue is inactive, don't schedule the item
                    ReadyInQueue = qr.State == QueueState.Active ? messageRecord.StartTimestamp.Value : null
                }
            ).ToArray();
        
        queueItemIds = 
            queueItems
            .Select(qir => qir.QueueItemId)
            .ToArray();
        queueItems.ForEach(qi => this.queueItems.Add(qi));

        return queueItemIds;
    }

    IMessageRecord IMessageEngine.GetMessage(Guid messageId) {
        var message = 
            messageRecords
            .Where(mr => mr.FabricMessage.MessageId == messageId)
            .FirstOrDefault();
        if (message == null) {
            return null;
        }
        var items = 
            queueItems
            .Where(qir => qir.MessageId == messageId)
            .ToArray();
        message.Results = items.SelectMany(i => i.Results ?? (Array.Empty<ServiceCallResult>())).ToArray();
        return message;
    }

    bool IMessageEngine.IsMessageDelivered(Guid messageId) {
        var unprocessedCount =
            queueItems
            .Where(qir => qir.MessageId == messageId)
            .Where(qir => qir.State != QueueItemState.Delivered && qir.State != QueueItemState.Dead)
            .Count();
        return unprocessedCount == 0;
    }

    // ---------------------------------------------------------------------
    // Item processing
    // ---------------------------------------------------------------------

    int IMessageEngine.GetReadyCount() {
        return queueItems
        .Where(qir => qir.ReadyInQueue < DateTime.UtcNow && qir.ReadyInQueue != null)
        .Count();
    }

    (IQueueItemRecord item, IReadOnlySubscription subscription, string jsonPayload) IMessageEngine.LockReadyQueueItem(Guid queueItemId) {
        IReadOnlySubscription subscription = null;
        string jsonPayload = null;
        QueueItemRecord readyItem = null;

        var now = DateTime.UtcNow;
        IEnumerable<QueueItemRecord> query = queueItems;
        if (queueItemId != default) {
            query = query.Where(qir => qir.QueueItemId == queueItemId);
        }
        query = query
            .Where(qir => qir.ReadyInQueue < now && qir.ReadyInQueue != null)
            .OrderBy(qir => qir.ReadyInQueue);
        readyItem = query.FirstOrDefault();
        if (readyItem != null) {
            readyItem.StartProcessingTimestamp = now;
            readyItem.State = QueueItemState.Processing;
            readyItem.ReadyInQueue = null;

            jsonPayload =
                messageRecords
                .Where(m => m.FabricMessage.MessageId == readyItem.MessageId)
                .Single()
                .FabricMessage.JsonPayload;
            subscription = 
                queues
                .Where(x => x.QueueName == readyItem.QueueName)
                .Single()
                .Subscription;
        }

        return (readyItem, subscription, jsonPayload);
    }

    IReadOnlyList<Guid> IMessageEngine.GetReadyQueueItemIds() {
        var now = DateTime.UtcNow;
        return queueItems
            .Where(qir => qir.ReadyInQueue < now && qir.ReadyInQueue != null)
            .OrderBy(qir => qir.ReadyInQueue)
            .Select(qir => qir.QueueItemId)
            .ToArray();
    }

    void IMessageEngine.ReleaseQueueItem(Guid queueItemId) {
        var item = 
            queueItems
            .Where(i => i.QueueItemId == queueItemId)
            .SingleOrDefault();
        if (item == null) {
            return;
        }

        item.StartProcessingTimestamp = null;
        item.State = QueueItemState.Enqueued;
        item.ReadyInQueue = item.SubmittedTimestamp;
    }

    void IMessageEngine.CompleteQueueItemAsDelivered(
        Guid queueItemId, 
        IReadOnlyList<ServiceCallResult> results
    ) {
        var item = 
            queueItems
            .Where(i => i.QueueItemId == queueItemId)
            .SingleOrDefault();
        if (item == null) {
            return;
        }

        item.FinishProcessingTimestamp = DateTime.UtcNow;
        item.State = QueueItemState.Delivered;
        item.ReadyInQueue = null;
        item.Results = results?.ToArray() ?? Array.Empty<ServiceCallResult>();;

        var queue = 
            queues
            .Where(qr => qr.QueueName == item.QueueName)
            .Single();
        queue.ConsecutiveFailureCount = 0;
    }

    IQueueItemRecord IMessageEngine.FailQueueItem(
        Guid queueItemId, 
        IReadOnlyList<ServiceCallResult> results
    ) {
        QueueItemRecord item;
        item = queueItems
            .Where(i => i.QueueItemId == queueItemId)
            .SingleOrDefault();
        if (item == null) {
            return null;
        }

        item.StartProcessingTimestamp = null;
        item.PreviousAttempts++;
        item.ReadyInQueue = null;
        item.Results = results?.ToArray() ?? Array.Empty<ServiceCallResult>();;

        var queue = 
            queues
            .Where(qr => qr.QueueName == item.QueueName)
            .Single();
        var failureDelays = 
            queue.Subscription.FailureDelaysToRetryMs ??
            parameters.DefaultDeliveryFailureDelaysToRetryMs;
        var maxQueueFailures = 
            queue.Subscription.MaxConsecutiveFailures ??
            parameters.DefaultMaxConsecutiveFailuresPerQueue;
        var itemFailureCount = item.PreviousAttempts;
        var maxRetries = 
            queue.Subscription.MaxDeliveryRetries ?? 
            parameters.DefaultMaxItemRetries;

        if (itemFailureCount > maxRetries) {

            item.State = QueueItemState.Dead;

            var m = "Queue item passed max retries";
            var attributes = new {
                Recipient = Identifiers.GetServiceFullRegistrationKey(queue.Subscription.Recipient),
                RecipientId = queue.Subscription.RecipientHostId ?? "",
                QueueName = item.QueueName,
                MessageName = queue.Subscription.MessageTypeName
            };

            // NOTE:  We are ignoring a few cases of queue.Subscription.ErrorHandling
            //        for the purpose of this sample
            switch(queue.Subscription.ErrorHandling) {
            case ServiceClientErrorHandling.ThrowException:
                throw new Exception(m);
            }
        } else {
            var delayUntilNextAttemptMs = 
                itemFailureCount >= failureDelays.Length ?
                failureDelays[^1] :
                failureDelays[itemFailureCount];
            TimeSpan delayUntilNextAttempt = TimeSpan.FromMilliseconds(delayUntilNextAttemptMs);
            item.ReadyInQueue = DateTime.UtcNow.Add(delayUntilNextAttempt);
            item.State = QueueItemState.Enqueued;
        }

        queue.ConsecutiveFailureCount++;

        if (queue.ConsecutiveFailureCount >= maxQueueFailures) {

            queue.State = QueueState.Inactive;

            // Unschedule all items
            var items = queueItems
                .Where(qir => qir.ReadyInQueue != null)
                .ToArray();
            items.ForEach(qir => qir.ReadyInQueue = null);
        }
        return item;
    }

    void IMessageEngine.KillQueueItem(
        Guid queueItemId, 
        IReadOnlyList<ServiceCallResult> results
    ) {
        var item = 
            queueItems
            .Where(i => i.QueueItemId == queueItemId)
            .SingleOrDefault();
        if (item == null) {
            return;
        }

        item.StartProcessingTimestamp = null;
        item.State = QueueItemState.Dead;
        item.PreviousAttempts++;
        item.ReadyInQueue = null;
        item.Results = results?.ToArray() ?? Array.Empty<ServiceCallResult>();;
    }

    // ---------------------------------------------------------------------
    // Other Item
    // ---------------------------------------------------------------------

    IReadOnlyList<IQueueItemRecord> IMessageEngine.GetQueueItemsForMessage(Guid messageId) {
        return 
            queueItems
            .Where(qir => qir.MessageId == messageId)
            .ToArray();
    }

    void IMessageEngine.DeleteQueueItems(
        DateTime olderThan,
        QueueItemState state
    ) {
        var items = 
            queueItems
            .Where(qir => qir.ReadyInQueue != null && qir.ReadyInQueue < olderThan && qir.State == state)                
            .ToArray();
        items.ForEach(qi => queueItems.Remove(qi));
    }
    
    // ---------------------------------------------------------------------
    // Database and other
    // ---------------------------------------------------------------------

    void IMessageEngine.SetParameters(MessageEngineParameters p) {
        if (p == null) { return; }
        parameters = p.DeepCopy();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Tests.Services.MessageBroker.TestMessages;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Services.MessageBroker;
using Samples.MessageBroker.Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Samples.MessageBroker.Tests.Engine;

[TestClass]
public class MessageEngineTests {

    [TestMethod]
    public void LoadsAndGetsQueues() => DoTest(engine => {
        
        // ---- Arrange 

        var subscriptions = new Subscription[] {
            new() {
                Recipient = TestServices.Constants.Service1.Clone(),
                MessageTypeName = $"{nameof(TestCommands)}.{nameof(TestCommands.Command1)}",
                RecipientHostId = "some host id",
                MaxConsecutiveFailures = 2,
                MaxDeliveryRetries = 4
            },
            new() {
                Recipient = TestServices.Constants.Service2.Clone(),
                MessageTypeName = $"{nameof(TestCommands)}.{nameof(TestCommands.Command2)}",
                RecipientHostId = "some host id2",
                MaxConsecutiveFailures = 2,
                MaxDeliveryRetries = 4
            }
        };

        // ---- Act

        engine.EnsureLoadQueues(subscriptions);
        var queues = engine.GetQueues();

        // ---- Assert

        queues.Should().BeEquivalentTo(
            new[] { 
                new QueueRecord {
                    QueueName = subscriptions[0].GetQueueName(),
                    Subscription = subscriptions[0]
                },
                new QueueRecord {
                    QueueName = subscriptions[1].GetQueueName(),
                    Subscription = subscriptions[1]
                }
            },
            opt => opt.Including(x => x.QueueName).Including(x => x.Subscription)
        );
    });

    [TestMethod]
    public void CreateSubscriptionAndGetQueue() => DoTest(engine => {

        // ---- Arrange 

        var subscriptions = AddTestSubscriptions(engine);
        var newSubscription = new Subscription {
            Recipient = TestServices.Constants.Service3.Clone(),
            MessageTypeName = $"{nameof(TestCommands)}.{nameof(TestCommands.Command1)}",
            RecipientHostId = "some host id",
            MaxConsecutiveFailures = 2,
            MaxDeliveryRetries = 4
        };
        subscriptions.Add(newSubscription);

        // ---- Act

        engine.CreateRenewSubscription(newSubscription);
        var queues = engine.GetQueues();
        var newQueue = engine.GetQueue(newSubscription.GetQueueName());

        // ---- Assert

        queues.Should().BeEquivalentTo(
            new[] { 
                new QueueRecord {
                    QueueName = subscriptions[0].GetQueueName(),
                    Subscription = subscriptions[0]
                },
                new QueueRecord {
                    QueueName = subscriptions[1].GetQueueName(),
                    Subscription = subscriptions[1]
                },
                new QueueRecord {
                    QueueName = subscriptions[2].GetQueueName(),
                    Subscription = subscriptions[2]
                }
            },
            opt => opt.Including(x => x.QueueName).Including(x => x.Subscription)
        );
        newQueue.Should().NotBeNull();
        newQueue.Should().BeEquivalentTo(
            new QueueRecord {
                QueueName = subscriptions[2].GetQueueName(),
                Subscription = subscriptions[2]
            },
            opt => opt.Including(x => x.QueueName).Including(x => x.Subscription)
        );
    });

    [TestMethod]
    public void DeactivateAndReactivateQueue() => DoTest(engine => {

        // ---- Arrange 

        var subscriptions = AddTestSubscriptions(engine);

        // ---- Act

        engine.DeactivateQueue(subscriptions[0].GetQueueName());
        engine.DeactivateQueue(subscriptions[1].GetQueueName());
        engine.CreateRenewSubscription(subscriptions[0]);

        // ---- Assert

        var queues = engine.GetQueues();
        queues.Should().BeEquivalentTo(
            new[] { 
                new QueueRecord {
                    QueueName = subscriptions[0].GetQueueName(),
                    Subscription = subscriptions[0]
                },
                new QueueRecord {
                    QueueName = subscriptions[1].GetQueueName(),
                    Subscription = subscriptions[1]
                }
            },
            opt => opt.Including(x => x.QueueName).Including(x => x.Subscription)
        );
        
        var active = queues.Where(q => q.QueueName == subscriptions[0].GetQueueName()).Single();
        var inactive = queues.Where(q => q.QueueName == subscriptions[1].GetQueueName()).Single();

        inactive.State.Should().Be(QueueState.Inactive);
        active.State.Should().Be(QueueState.Active);
    });

    [TestMethod]
    public void DeactivatedQueuesAccumulateItemsButDoNotDeliver() => DoTest(engine => {

        // ---- Arrange 

        var subscriptions = AddTestSubscriptions(engine);

        // ---- Act

        engine.DeactivateQueue(subscriptions[0].GetQueueName());

        // ---- Assert

        var (msg1, _) = AddTestMessage(engine);
        engine.GetReadyCount().Should().Be(0);

        AddTestMessage(engine);
        engine.GetReadyCount().Should().Be(0);

        var items = engine.GetQueueItemsForMessage(msg1.FabricMessage.MessageId);
        items.Count.Should().Be(1);
        items[0].MessageId.Should().Be(msg1.FabricMessage.MessageId);

        // trying to lock this item does not work.. the queue is deactivated
        var (nullQueueItem, _, _) = engine.LockReadyQueueItem(items[0].QueueItemId);
        nullQueueItem.Should().BeNull();
    });

    [TestMethod]
    public void AbandonedQueuesDeliverMessagesButDoNotAccumulateNew() => DoTest(engine => {
        
        // ---- Arrange 

        var subscriptions = AddTestSubscriptions(engine);
        var (_, queueItemIds) = AddTestMessage(engine);

        // ---- Act

        engine.AbandonQueue(subscriptions[0].GetQueueName());

        // ---- Assert

        (engine.GetReadyCount()).Should().Be(1);

        // should not add message to abandoned queue
        AddTestMessage(engine);
        (engine.GetReadyCount()).Should().Be(1);

        var nonNullQueueItem = engine.LockReadyQueueItem(queueItemIds[0]);
        nonNullQueueItem.Should().NotBeNull();
        (engine.GetReadyCount()).Should().Be(0);
    });

    [TestMethod]
    public void GetQueuesReturnsNonAbandonedQueues() => DoTest(engine => {
        
        // ---- Arrange 

        var subscriptions = AddTestSubscriptions(engine);

        // ---- Act

        engine.AbandonQueue(subscriptions[0].GetQueueName());

        // ---- Assert

        var queues = engine.GetQueues();
        queues.Should().BeEquivalentTo(
            new[] { 
                new QueueRecord {
                    QueueName = subscriptions[1].GetQueueName(),
                    Subscription = subscriptions[1]
                },
            },
            opt => opt.Including(x => x.QueueName).Including(x => x.Subscription)
        );
    });

    [TestMethod]
    public void CreateAndLockQueueItems() => DoTest(engine => {

        // ---- Arrange 

        var messageTypeName = $"{nameof(TestCommands)}.{nameof(TestCommands.Command1)}";
        var subscriptions = new Subscription[] {
            AddTestSubscription(engine, TestServices.Constants.Service1, messageTypeName),
            AddTestSubscription(engine, TestServices.Constants.Service2, messageTypeName),
            AddTestSubscription(engine, TestServices.Constants.Service2, $"{nameof(TestCommands)}.{nameof(TestCommands.Command2)}")
        };

        // ---- Act 
        var msg1 = new MessageRecord {
            Kind = MessageKind.Command,
            LocalOperationId = Guid.NewGuid(),
            FabricMessage = new FabricMessage {
                JsonPayload = "{ }",
                MessageId = Guid.NewGuid(),
                MessageTypeName = messageTypeName,
                OriginatorCorrelationId = Guid.NewGuid().ToString(),
                OriginatorRequestorFabricId = Guid.NewGuid().ToString(),
                OriginatorRequestorInstanceId = Guid.NewGuid().ToString()
            }
        };
        var msg2 = new MessageRecord {
            Kind = MessageKind.Command,
            LocalOperationId = Guid.NewGuid(),
            FabricMessage = new FabricMessage {
                JsonPayload = "{ }",
                MessageId = Guid.NewGuid(),
                MessageTypeName = messageTypeName,
                OriginatorCorrelationId = Guid.NewGuid().ToString(),
                OriginatorRequestorFabricId = Guid.NewGuid().ToString(),
                OriginatorRequestorInstanceId = Guid.NewGuid().ToString()
            }
        };

        var queueItemIdsMsg1 = engine.AddMessage(msg1);
        var queueItemIdsMsg2 = engine.AddMessage(msg2);

        // ---- Assert

        (engine.GetReadyCount()).Should().Be(4);

        var itemsMsg1 = engine.GetQueueItemsForMessage(msg1.FabricMessage.MessageId);
        itemsMsg1.Count.Should().Be(2);
        queueItemIdsMsg1.Count.Should().Be(2);
        queueItemIdsMsg1.Should().Contain(itemsMsg1[0].QueueItemId);
        queueItemIdsMsg1.Should().Contain(itemsMsg1[1].QueueItemId);
        itemsMsg1[0].MessageId.Should().Be(msg1.FabricMessage.MessageId);
        itemsMsg1[1].MessageId.Should().Be(msg1.FabricMessage.MessageId);

        var itemsMsg2 = engine.GetQueueItemsForMessage(msg2.FabricMessage.MessageId);
        itemsMsg2[0].MessageId.Should().Be(msg2.FabricMessage.MessageId);
        itemsMsg2[1].MessageId.Should().Be(msg2.FabricMessage.MessageId);

        var queueName = subscriptions[0].GetQueueName();
        var queue = engine.GetQueue(queueName);

        IQueueItemRecord queueItem;
        IReadOnlySubscription subscription;
        string json;
        (queueItem, subscription, json) = engine.LockReadyQueueItem(itemsMsg1[0].QueueItemId);
        queueItem.Should().NotBeNull();
        queueItem.State.Should().Be(QueueItemState.Processing);
        subscription.Should().NotBeNull();
        subscription.MessageTypeName.Should().Be(queueItem.MessageTypeName);
        json.Should().NotBeNullOrEmpty();
        engine.GetReadyCount().Should().Be(3);
        engine.GetQueueItemsForMessage(msg1.FabricMessage.MessageId)
            .Where(qir => qir.State == QueueItemState.Enqueued)
            .Count().Should().Be(1);

        (queueItem, subscription, json) = engine.LockReadyQueueItem(itemsMsg1[1].QueueItemId);
        queueItem.Should().NotBeNull();
        queueItem.State.Should().Be(QueueItemState.Processing);
        subscription.Should().NotBeNull();
        subscription.MessageTypeName.Should().Be(queueItem.MessageTypeName);
        json.Should().NotBeNullOrEmpty();
        (engine.GetReadyCount()).Should().Be(2);
        (engine.GetQueueItemsForMessage(msg1.FabricMessage.MessageId))
            .Where(qir => qir.State == QueueItemState.Enqueued)
            .Count().Should().Be(0);
    });

    [TestMethod]
    public void MessagesMarkedAsComplete() => DoTest(engine => {

        // ---- Arrange 

        var messageTypeName = $"{nameof(TestCommands)}.{nameof(TestCommands.Command1)}";
        AddTestSubscription(engine, TestServices.Constants.Service1, messageTypeName);
        AddTestSubscription(engine, TestServices.Constants.Service2, messageTypeName);
        AddTestSubscription(engine, TestServices.Constants.Service3, messageTypeName);

        var (msg, queueItemIds) = AddTestMessage(engine);
        var queue = engine.GetQueue(messageTypeName);
        var result = new ServiceCallResult { 
            Code = "test",
            ServiceCallStatus = ServiceCallStatusEnum.Completed
        };

        // ---- Act / Assert

        IMessageRecord message;
        IQueueItemRecord queueItem;
        (queueItem, _, _) = engine.LockReadyQueueItem(queueItemIds[0]);
        engine.CompleteQueueItemAsDelivered(queueItem.QueueItemId, new[] { result });
        message = engine.GetMessage(queueItem.MessageId);
        message.Results.Should().BeEquivalentTo(new[] { result });
        (engine.IsMessageDelivered(queueItem.MessageId)).Should().Be(false);
        
        (queueItem, _, _) = engine.LockReadyQueueItem(queueItemIds[1]);
        engine.KillQueueItem(queueItem.QueueItemId, new[] { result });
        message = engine.GetMessage(queueItem.MessageId);
        message.Results.Should().BeEquivalentTo(new[] { result, result });
        (engine.IsMessageDelivered(queueItem.MessageId)).Should().Be(false);
        
        (queueItem, _, _) = engine.LockReadyQueueItem(queueItemIds[2]);
        engine.CompleteQueueItemAsDelivered(queueItem.QueueItemId, new[] { result });
        message = engine.GetMessage(queueItem.MessageId);
        message.Results.Should().BeEquivalentTo(new[] { result, result, result });
        (engine.IsMessageDelivered(queueItem.MessageId)).Should().Be(true);
    });

    [TestMethod]
    public void ReleaseQueueItemReturnsToReady() => DoTest(engine => {

        // ---- Arrange 

        var subscriptions = AddTestSubscriptions(engine);
        var (_, queueItemIds) = AddTestMessage(engine);
        var (item, _, _) = engine.LockReadyQueueItem(queueItemIds[0]);

        // ---- Act 

        (engine.GetReadyCount()).Should().Be(0);
        engine.ReleaseQueueItem(item.QueueItemId);

        // ---- Assert

        (engine.GetReadyCount()).Should().Be(1);
        (engine.GetReadyQueueItemIds()).Should().BeEquivalentTo(new[] { item.QueueItemId });
    });

    [TestMethod]
    public void FailedQueueItemsWaitForRetry() => DoTest(engine => {

        // ---- Arrange 

        var messageTypeName = $"{nameof(TestCommands)}.{nameof(TestCommands.Command1)}";
        var subscriptions = new List<Subscription> {
            new() {
                Recipient = TestServices.Constants.Service1.Clone(),
                MessageTypeName = messageTypeName,
                RecipientHostId = "some host id",
                MaxConsecutiveFailures = 4,
                MaxDeliveryRetries = 5,
                FailureDelaysToRetryMs = new[] { 2000 } 
            }
        };

        engine.EnsureLoadQueues(subscriptions);

        var queueName = subscriptions[0].GetQueueName();
        var (_, queueItemIds1stMessage) = AddTestMessage(engine, messageTypeName);
        var (_, queueItemIds2ndMessage) = AddTestMessage(engine, messageTypeName);
        var (_, queueItemIds3rdMessage) = AddTestMessage(engine, messageTypeName);

        IQueueRecord queue;
        engine.GetReadyCount().Should().Be(3);

        // ---- Act / Assert

        var (item1, _, _) = engine.LockReadyQueueItem(queueItemIds1stMessage[0]);
        engine.FailQueueItem(
            item1.QueueItemId, 
            null
        );
        queue = engine.GetQueue(queueName);
        queue.ConsecutiveFailureCount.Should().Be(1);
        // this was scheduled in to the future so should not be present
        (engine.GetReadyQueueItemIds()).Should().NotContain(item1.QueueItemId);
        (engine.LockReadyQueueItem(item1.QueueItemId)).item.Should().BeNull();

        var (item2, _, _) = engine.LockReadyQueueItem(queueItemIds2ndMessage[0]);
        item2.QueueItemId.Should().NotBe(item1.QueueItemId);
        engine.FailQueueItem(
            item2.QueueItemId, 
            null
        );
        queue = engine.GetQueue(queueName);
        queue.ConsecutiveFailureCount.Should().Be(2);
        // this was scheduled in to the future so should not be present
        (engine.GetReadyQueueItemIds()).Should().NotContain(item2.QueueItemId);
        (engine.LockReadyQueueItem(item2.QueueItemId)).item.Should().BeNull();

        // Do a success case
        var (item3, _, _) = engine.LockReadyQueueItem(queueItemIds3rdMessage[0]);
        engine.CompleteQueueItemAsDelivered(item3.QueueItemId, null);
        queue = engine.GetQueue(queueName);
        queue.ConsecutiveFailureCount.Should().Be(0);


        // now that time has passed, item1 should again be ready
        Thread.Sleep(2500);
        (engine.GetReadyQueueItemIds()).Should().Contain(item1.QueueItemId);
        (engine.GetReadyQueueItemIds()).Should().Contain(item2.QueueItemId);
        (engine.LockReadyQueueItem(item1.QueueItemId)).item.Should().NotBeNull();
    });

    [TestMethod]
    public void FailingQueueItemsDeactivatesQueue() => DoTest(engine => {

        // ---- Arrange 

        var messageTypeName = $"{nameof(TestCommands)}.{nameof(TestCommands.Command1)}";
        var subscriptions = new List<Subscription> {
            new() {
                Recipient = TestServices.Constants.Service1.Clone(),
                MessageTypeName = messageTypeName,
                RecipientHostId = "some host id",
                MaxConsecutiveFailures = 2,
                MaxDeliveryRetries = 5,
                FailureDelaysToRetryMs = new[] { 200 } 
            }
        };

        engine.EnsureLoadQueues(subscriptions);

        var queueName = subscriptions[0].GetQueueName();
        var (_, queueItemIds1stMessage) = AddTestMessage(engine, messageTypeName);
        var (_, queueItemIds2ndMessage) = AddTestMessage(engine, messageTypeName);
        var (_, queueItemIds3rdMessage) = AddTestMessage(engine, messageTypeName);

        IQueueRecord queue;
        (engine.GetReadyCount()).Should().Be(3);

        // ---- Act / Assert

        var (item1, _, _) = engine.LockReadyQueueItem(queueItemIds1stMessage[0]);
        engine.FailQueueItem(
            item1.QueueItemId, 
            null
        );
        var (item2, _, _) = engine.LockReadyQueueItem(queueItemIds2ndMessage[0]);
        item2.QueueItemId.Should().NotBe(item1.QueueItemId);
        engine.FailQueueItem(
            item2.QueueItemId, 
            null
        );
        queue = engine.GetQueue(queueName);
        queue.ConsecutiveFailureCount.Should().Be(2);
        queue.State.Should().Be(QueueState.Inactive);
    });

    [TestMethod]
    public void SuccessfulQueueItemPreventsDeactivatingQueue() => DoTest(engine => {

        // ---- Arrange 

        var messageTypeName = $"{nameof(TestCommands)}.{nameof(TestCommands.Command1)}";
        var subscriptions = new List<Subscription> {
            new() {
                Recipient = TestServices.Constants.Service1.Clone(),
                MessageTypeName = messageTypeName,
                RecipientHostId = "some host id",
                MaxConsecutiveFailures = 2,
                MaxDeliveryRetries = 5,
                FailureDelaysToRetryMs = new[] { 200 } 
            }
        };

        engine.EnsureLoadQueues(subscriptions);

        var queueName = subscriptions[0].GetQueueName();
        var (_, queueItemIds1stMessage) = AddTestMessage(engine, messageTypeName);
        var (_, queueItemIds2ndMessage) = AddTestMessage(engine, messageTypeName);
        var (_, queueItemIds3rdMessage) = AddTestMessage(engine, messageTypeName);

        IQueueRecord queue;
        (engine.GetReadyCount()).Should().Be(3);

        // ---- Act / Assert

        var (item1, _, _) = engine.LockReadyQueueItem(queueItemIds1stMessage[0]);
        engine.FailQueueItem(
            item1.QueueItemId, 
            null
        );
        var (item2, _, _) = engine.LockReadyQueueItem(queueItemIds2ndMessage[0]);
        item2.QueueItemId.Should().NotBe(item1.QueueItemId);
        engine.CompleteQueueItemAsDelivered(item2.QueueItemId, null);

        var (item3, _, _) = engine.LockReadyQueueItem(queueItemIds3rdMessage[0]);
        engine.FailQueueItem(
            item3.QueueItemId, 
            null
        );

        queue = engine.GetQueue(queueName);
        queue.ConsecutiveFailureCount.Should().Be(1);
        queue.State.Should().Be(QueueState.Active);        
    });

    [TestMethod]
    public void FailingQueueItemsKillsItems() => DoTest(engine => {

        // ---- Arrange 

        var messageTypeName = $"{nameof(TestCommands)}.{nameof(TestCommands.Command1)}";
        var subscriptions = new List<Subscription> {
            new() {
                Recipient = TestServices.Constants.Service1.Clone(),
                MessageTypeName = messageTypeName,
                RecipientHostId = "some host id",
                MaxConsecutiveFailures = 5,
                MaxDeliveryRetries = 1,
                FailureDelaysToRetryMs = new[] { 1 } 
            }
        };

        engine.EnsureLoadQueues(subscriptions);

        var queueName = subscriptions[0].GetQueueName();
        var (_, queueItemIds1stMessage) = AddTestMessage(engine, messageTypeName);
        var (_, queueItemIds2ndMessage) = AddTestMessage(engine, messageTypeName);

        (engine.GetReadyCount()).Should().Be(2);

        // ---- Act / Assert

        var (item1, _, _) = engine.LockReadyQueueItem(queueItemIds1stMessage[0]);
        engine.FailQueueItem(
            item1.QueueItemId, 
            null
        );
        Thread.Sleep(100);
        var (item1SecondTime, _, _) = engine.LockReadyQueueItem(queueItemIds1stMessage[0]);
        engine.FailQueueItem(
            item1.QueueItemId, 
            null
        );
        Thread.Sleep(100);
        var (item1ThirdTime, _, _) = engine.LockReadyQueueItem(queueItemIds1stMessage[0]);
        item1ThirdTime.Should().BeNull();

        (engine.GetReadyQueueItemIds()).Should().NotContain(queueItemIds1stMessage[0]);
        (engine.GetReadyCount()).Should().Be(1);
    });

    [TestMethod]
    public void KillQueueItem() => DoTest(engine => {
        // ---- Arrange 

        var subscriptions = AddTestSubscriptions(engine);
        var queueName = subscriptions[0].GetQueueName();
        var (_, queueItemIds) = AddTestMessage(engine);
        IQueueRecord queue;
        queue = engine.GetQueue(queueName);
        (engine.GetReadyCount()).Should().Be(1);

        // ---- Act / Assert

        var (item1, _, _) = engine.LockReadyQueueItem(queueItemIds[0]);
        engine.KillQueueItem(
            item1.QueueItemId, 
            null
        );
        (engine.GetReadyCount()).Should().Be(0);
    });

    [TestMethod]
    public void DeleteQueueItems() => DoTest(engine => {
        // ---- Arrange 

        var subscriptions = AddTestSubscriptions(engine);
        var queueName = subscriptions[0].GetQueueName();
        AddTestMessage(engine);
        AddTestMessage(engine);
        Thread.Sleep(100);
        var beforeTime = DateTime.UtcNow;
        Thread.Sleep(100);
        AddTestMessage(engine);

        // ---- Act / Assert

        engine.DeleteQueueItems(
            beforeTime, 
            QueueItemState.Enqueued
        );
        (engine.GetReadyCount()).Should().Be(1);
    });

    // =========================================================================
    // private helpers
    // =========================================================================

    private static void DoTest(
        Action<IMessageEngine> test
    ) {
        var engine = new MessageEngine();
        test(engine);
    }

    private List<Subscription> AddTestSubscriptions(IMessageEngine engine) {
        var subscriptions = new List<Subscription> {
            new() {
                Recipient = TestServices.Constants.Service1.Clone(),
                MessageTypeName = $"{nameof(TestCommands)}.{nameof(TestCommands.Command1)}",
                RecipientHostId = "some host id",
                MaxConsecutiveFailures = 3,
                MaxDeliveryRetries = 4,
                FailureDelaysToRetryMs = new[] { 2000 } 
            },
            new() {
                Recipient = TestServices.Constants.Service2.Clone(),
                MessageTypeName = $"{nameof(TestCommands)}.{nameof(TestCommands.Command2)}",
                RecipientHostId = "some host id2",
                MaxConsecutiveFailures = 2,
                MaxDeliveryRetries = 4,
                FailureDelaysToRetryMs = new[] { 10000 } 
            }
        };

        engine.EnsureLoadQueues(subscriptions);
        return subscriptions;
    }

    private Subscription AddTestSubscription(
        IMessageEngine engine,
        IReadOnlyDescriptor recipientService = null, 
        string messageTypeName = null
    ) {
        var subscriptions = new List<Subscription> {
            new() {
                Recipient = recipientService?.Clone() ?? TestServices.Constants.Service1.Clone(),
                MessageTypeName = messageTypeName ?? $"{nameof(TestCommands)}.{nameof(TestCommands.Command1)}",
                RecipientHostId = "some host id",
                MaxConsecutiveFailures = 2,
                MaxDeliveryRetries = 4
            }
        };

        engine.EnsureLoadQueues(subscriptions);
        return subscriptions[0];
    }

    private (MessageRecord msg, IReadOnlyList<Guid> queueItemIds) AddTestMessage(
        IMessageEngine engine, 
        string messageTypeName = null
    ) {
        messageTypeName ??= $"{nameof(TestCommands)}.{nameof(TestCommands.Command1)}";
        var msg = new MessageRecord {
            Kind = MessageKind.Command,
            LocalOperationId = Guid.NewGuid(),
            FabricMessage = new FabricMessage {
                JsonPayload = "{ }",
                MessageId = Guid.NewGuid(),
                MessageTypeName = messageTypeName,
                OriginatorCorrelationId = Guid.NewGuid().ToString(),
                OriginatorRequestorFabricId = Guid.NewGuid().ToString(),
                OriginatorRequestorInstanceId = Guid.NewGuid().ToString()
            }
        };
        var queueItemIds = engine.AddMessage(msg);
        return (msg, queueItemIds);
    }
}

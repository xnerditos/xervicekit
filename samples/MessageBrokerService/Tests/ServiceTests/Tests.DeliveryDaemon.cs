using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.Services.MessageBroker.TestMessages;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Services.MessageBroker;
using XKit.Lib.Common.Utility;
using XKit.Lib.Common.Utility.Extensions;
using Samples.MessageBroker.Engine;

namespace Samples.MessageBroker.Tests.Daemon;
[TestClass]
public class DeliveryDaemon : TestBase {
    
    [ClassInitialize]
    public static void ClassInitialize(TestContext _) { }

    [ClassCleanup]
    public static void ClassCleanup() {}

    [TestMethod]
    public async Task MessageRetriesAndSucceeds() {
        TestBase.InitTests();
        Daemon.SetDebugMode(true);

        try {
            SetRuntimeConfiguration(
                servicesConfig: new Dictionary<IReadOnlyDescriptor, object> {
                    {
                        Samples.MessageBroker.Constants.ServiceDescriptor,
                        new MessageBrokerConfig {
                            DeliveryDaemon = new MessageBrokerConfig.DeliveryDaemonType {
                                DefaultDeliveryFailureDelaysToRetryMs = new[] { -10 }, // ensure always ready to try again right away
                                DefaultMaxConsecutiveFailuresPerQueue = 4,
                                DefaultMaxItemRetries = 3
                            }                           
                        }
                    }
                }
            );

            Svc3.Command2FailCount = 2;
            (FabricMessage message, TestPayload payload) = await SendCommand(nameof(TestCommands.Command2));
            DispatchDeliveryDaemonMessagesAndTimerEvents();
            
            var finishedMessage = Engine.GetMessage(message.MessageId);
            finishedMessage.Results.Count.Should().Be(1);
            finishedMessage.Results[0].HasError.Should().BeFalse();
            var items = Engine.GetQueueItemsForMessage(message.MessageId);
            items.Count.Should().Be(1);
            items[0].State.Should().Be(QueueItemState.Delivered);
        } finally {
            TestBase.TeardownTests();
        }
    }

    [TestMethod]
    public async Task MessageRetriesAndDies() {
        TestBase.InitTests();
        Daemon.SetDebugMode(true);
        try {
            SetRuntimeConfiguration(
                servicesConfig: new Dictionary<IReadOnlyDescriptor, object> {
                    {
                        Samples.MessageBroker.Constants.ServiceDescriptor,
                        new MessageBrokerConfig {
                            DeliveryDaemon = new MessageBrokerConfig.DeliveryDaemonType {
                                DefaultDeliveryFailureDelaysToRetryMs = new[] { -10 }, // ensure always ready to try again right away
                                DefaultMaxConsecutiveFailuresPerQueue = 3,
                                DefaultMaxItemRetries = 2
                            }                           
                        }
                    }
                }
            );

            Svc3.Command2FailCount = 3;
            (FabricMessage message, TestPayload payload) = await SendCommand(nameof(TestCommands.Command2));
            DispatchDeliveryDaemonMessagesAndTimerEvents();

            var finishedMessage = Engine.GetMessage(message.MessageId);
            finishedMessage.Results.Count.Should().Be(1);
            finishedMessage.Results[0].HasError.Should().BeTrue();
            var items = Engine.GetQueueItemsForMessage(message.MessageId);
            items.Count.Should().Be(1);
            items[0].State.Should().Be(QueueItemState.Dead);

        } finally {
            TestBase.TeardownTests();
        }
    }

    [TestMethod]
    public async Task MessageRetriesAndQueueDeactivated() {
        TestBase.InitTests();
        Daemon.SetDebugMode(true);
        try {

            SetRuntimeConfiguration(
                servicesConfig: new Dictionary<IReadOnlyDescriptor, object> {
                    {
                        Samples.MessageBroker.Constants.ServiceDescriptor,
                        new MessageBrokerConfig {
                            DeliveryDaemon = new MessageBrokerConfig.DeliveryDaemonType {
                                DefaultDeliveryFailureDelaysToRetryMs = new[] { -10 }, // ensure always ready to try again right away
                                DefaultMaxConsecutiveFailuresPerQueue = 2,
                                DefaultMaxItemRetries = 3
                            }                           
                        }
                    }
                }
            );

            Svc3.Command2FailCount = 3;
            (FabricMessage message, TestPayload payload) = await SendCommand(nameof(TestCommands.Command2));
            DispatchDeliveryDaemonMessagesAndTimerEvents();

            var queue = 
                (Engine.GetQueues())
                .Single(
                    q => q.Subscription.Recipient.Name == "Service3" &&
                         q.Subscription.MessageTypeName.Contains(nameof(TestCommands.Command2))
                );
            queue.State.Should().Be(QueueState.Inactive);

            var items = Engine.GetQueueItemsForMessage(message.MessageId);
            items.Count.Should().Be(1);
            items[0].State.Should().Be(QueueItemState.Enqueued);
            items[0].ReadyInQueue.HasValue.Should().BeFalse();

        } finally {
            TestBase.TeardownTests();
        }
    }

    [TestMethod]
    public async Task MultipleFailedItemsDeactivateQueue() {
        TestBase.InitTests();
        Daemon.SetDebugMode(true);
        try {

            SetRuntimeConfiguration(
                servicesConfig: new Dictionary<IReadOnlyDescriptor, object> {
                    {
                        Constants.ServiceDescriptor,
                        new MessageBrokerConfig {
                            DeliveryDaemon = new MessageBrokerConfig.DeliveryDaemonType {
                                DefaultDeliveryFailureDelaysToRetryMs = new[] { -10 }, // ensure always ready to try again right away
                                DefaultMaxConsecutiveFailuresPerQueue = 3,
                                DefaultMaxItemRetries = 1
                            }                           
                        }
                    }
                }
            );

            Svc3.Command2FailCount = 2;
            (FabricMessage message1, TestPayload payload1) = await SendCommand(nameof(TestCommands.Command2));
            DispatchDeliveryDaemonMessagesAndTimerEvents(1);
            (Engine.GetQueues())
            .Single(
                q => q.Subscription.Recipient.Name == "Service3" &&
                     q.Subscription.MessageTypeName.Contains(nameof(TestCommands.Command2))
            ).State.Should().Be(QueueState.Active);
            (Engine.GetQueueItemsForMessage(message1.MessageId))[0]
                .State.Should().Be(QueueItemState.Dead);

            Svc3.Command2FailCount = 2;
            (FabricMessage message2, TestPayload payload2) = await SendCommand(nameof(TestCommands.Command2));
            DispatchDeliveryDaemonMessagesAndTimerEvents();
            (Engine.GetQueues())
            .Single(
                q => q.Subscription.Recipient.Name == "Service3" &&
                     q.Subscription.MessageTypeName.Contains(nameof(TestCommands.Command2))
            ).State.Should().Be(QueueState.Inactive);
            // this one stays enqueued because the queue deactivates before it is killed.
            (Engine.GetQueueItemsForMessage(message2.MessageId))[0]
                .State.Should().Be(QueueItemState.Enqueued);

        } finally {
            TestBase.TeardownTests();
        }
    }

    [TestMethod]
    public async Task MultipleFailedItemsWithInterveningSuccessDoNotDeactivateQueue() {
        TestBase.InitTests();
        Daemon.SetDebugMode(true);
        try {

            SetRuntimeConfiguration(
                servicesConfig: new Dictionary<IReadOnlyDescriptor, object> {
                    {
                        Samples.MessageBroker.Constants.ServiceDescriptor,
                        new MessageBrokerConfig {
                            DeliveryDaemon = new MessageBrokerConfig.DeliveryDaemonType {
                                DefaultDeliveryFailureDelaysToRetryMs = new[] { -10 }, // ensure always ready to try again right away
                                DefaultMaxConsecutiveFailuresPerQueue = 3,
                                DefaultMaxItemRetries = 1
                            }                           
                        }
                    }
                }
            );

            Svc3.Command2FailCount = 2;
            (FabricMessage message1, TestPayload payload1) = await SendCommand(nameof(TestCommands.Command2));
            DispatchDeliveryDaemonMessagesAndTimerEvents(1);
            (Engine.GetQueueItemsForMessage(message1.MessageId))[0]
                .State.Should().Be(QueueItemState.Dead);

            Svc3.Command2FailCount = 0;
            (FabricMessage message2, TestPayload payload2) = await SendCommand(nameof(TestCommands.Command2));
            DispatchDeliveryDaemonMessagesAndTimerEvents(1);
            (Engine.GetQueueItemsForMessage(message2.MessageId))[0]
                .State.Should().Be(QueueItemState.Delivered);

            Svc3.Command2FailCount = 2;
            (FabricMessage message3, TestPayload payload3) = await SendCommand(nameof(TestCommands.Command2));
            DispatchDeliveryDaemonMessagesAndTimerEvents(1);
            (Engine.GetQueues())
            .Single(
                q => q.Subscription.Recipient.Name == "Service3" &&
                        q.Subscription.MessageTypeName.Contains(nameof(TestCommands.Command2))
            ).State.Should().Be(QueueState.Active);
            (Engine.GetQueueItemsForMessage(message3.MessageId))[0]
                .State.Should().Be(QueueItemState.Dead);
        } finally {
            TestBase.TeardownTests();
        }
    }

    [TestMethod]
    public async Task DeactivateQueuesAccumulateMessagesAndDeliveryOnNewSubscription() {
        TestBase.InitTests();
        Daemon.SetDebugMode(true);
        try {
            SetRuntimeConfiguration(
                servicesConfig: new Dictionary<IReadOnlyDescriptor, object> {
                    {
                        Constants.ServiceDescriptor,
                        new MessageBrokerConfig {
                            DeliveryDaemon = new MessageBrokerConfig.DeliveryDaemonType {
                                DefaultDeliveryFailureDelaysToRetryMs = new[] { -10 }, // ensure always ready to try again right away
                                DefaultMaxConsecutiveFailuresPerQueue = 1,
                                DefaultMaxItemRetries = 3
                            }                           
                        }
                    }
                }
            );

            Svc3.Command2FailCount = 2;
            (FabricMessage message1, TestPayload _) = await SendCommand(nameof(TestCommands.Command2));
            (FabricMessage message2, TestPayload _) = await SendCommand(nameof(TestCommands.Command2));
            DispatchDeliveryDaemonMessages();
            (Engine.GetQueues()).Single(
                q => q.Subscription.Recipient.Name == "Service3" &&
                     q.Subscription.MessageTypeName.Contains(nameof(TestCommands.Command2))
            ).State.Should().Be(QueueState.Inactive);
            (Engine.GetQueueItemsForMessage(message1.MessageId))[0]
                .State.Should().Be(QueueItemState.Enqueued);

            // this message should never even reach the service at this point
            Svc3.Command2FailCount = 0;     
            (FabricMessage message3, TestPayload _) = await SendCommand(nameof(TestCommands.Command2));
            DispatchDeliveryDaemonMessages();
            (Engine.GetQueueItemsForMessage(message3.MessageId))[0]
                .State.Should().Be(QueueItemState.Enqueued);

            // this should reactivate the queue and the messages should get delivered
            Engine.CreateRenewSubscription(new Subscription {
                Recipient = TestServices.Constants.Service3.Clone(),
                MessageTypeName = nameof(TestCommands) + "." + nameof(TestCommands.Command2)
            });

            (Engine.GetQueues()).Single(
                q => q.Subscription.Recipient.Name == "Service3" &&
                     q.Subscription.MessageTypeName.Contains(nameof(TestCommands.Command2))
            ).State.Should().Be(QueueState.Active);

            DispatchDeliveryDaemonMessagesAndTimerEvents(1);

            (Engine.GetQueueItemsForMessage(message1.MessageId))[0]
                .State.Should().Be(QueueItemState.Delivered);
            (Engine.GetQueueItemsForMessage(message2.MessageId))[0]
                .State.Should().Be(QueueItemState.Delivered);
            (Engine.GetQueueItemsForMessage(message3.MessageId))[0]
                .State.Should().Be(QueueItemState.Delivered);

        } finally {
            TestBase.TeardownTests();
        }
    }

    [TestMethod]
    public async Task PostponedMessagesWaitUntilTime() {

        TestBase.InitTests();
        Daemon.SetDebugMode(true);
        try {
            SetRuntimeConfiguration(
                servicesConfig: new Dictionary<IReadOnlyDescriptor, object> {
                    {
                        Samples.MessageBroker.Constants.ServiceDescriptor,
                        new MessageBrokerConfig {
                            HousekeepingDaemon = new MessageBrokerConfig.HousekeepingDaemonType {
                                Enable = false
                            },
                            DeliveryDaemon = new MessageBrokerConfig.DeliveryDaemonType {
                                MaxConcurrentMessages = 1,
                                DefaultDeliveryFailureDelaysToRetryMs = new[] { 900 }, 
                                DefaultMaxConsecutiveFailuresPerQueue = 2,
                                DefaultMaxItemRetries = 4
                            }                           
                        }
                    }
                }
            );

            Svc3.Command2FailCount = 1;
            (FabricMessage message1, TestPayload payload1) = await SendCommand(nameof(TestCommands.Command2));
            DispatchDeliveryDaemonMessages();
            (Engine.GetQueueItemsForMessage(message1.MessageId))[0]
                .State.Should().Be(QueueItemState.Enqueued);

            await Task.Delay(1000);
            DispatchDeliveryDaemonMessagesAndTimerEvents();

            var state = (Engine.GetQueueItemsForMessage(message1.MessageId))[0].State;
            state.Should().Be(QueueItemState.Delivered);

        } finally {
            TestBase.TeardownTests();
        }
    }

    private Task<(FabricMessage message, TestPayload payload)> SendEvent(
        string callMethodName
    ) => SendMessage(nameof(TestEvents), callMethodName);

    private Task<(FabricMessage message, TestPayload payload)> SendCommand(
        string callMethodName
    ) => SendMessage(nameof(TestCommands), callMethodName);

    private async Task<(FabricMessage message, TestPayload payload)> SendMessage(
        string callInterfaceName,
        string callMethodName
    ) {
        var client = CreateClient();

        var payload = new TestPayload {
            SomeValueGuid = Guid.NewGuid()
        };

        var messageId = Guid.NewGuid();
        var message = new FabricMessage {
            MessageId = messageId,
            JsonPayload = payload.ToJson(),
            MessageTypeName = $"{callInterfaceName}.{callMethodName}",
            OriginatorCorrelationId = Identifiers.GenerateIdentifier(),
            OriginatorRequestorFabricId = Identifiers.GenerateIdentifier(),
            OriginatorRequestorInstanceId = Identifiers.GenerateIdentifier()
        };
        var result = await client.IssueCommand(message);
        result.HasError.Should().BeFalse();
        return (message, payload);
    }
}

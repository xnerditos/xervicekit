using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.Services.MessageBroker.TestMessages;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Services.MessageBroker;
using XKit.Lib.Common.Utility;
using XKit.Lib.Common.Utility.Extensions;

namespace Samples.MessageBroker.Tests.Api;

[TestClass]
public class WaitOnMessage : TestBase {
    [ClassInitialize]
    public static void ClassInitialize(TestContext _) {
        TestBase.InitTests();
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
                            DefaultDeliveryFailureDelaysToRetryMs = new int[] { 100 }, // ensure always ready to try again right away
                            DefaultMaxConsecutiveFailuresPerQueue = 3,
                            DefaultMaxItemRetries = 2
                        }
                    }
                }
            }
        );
    }

    [ClassCleanup]
    public static void ClassCleanup() {
        TestBase.TeardownTests();
    }

    [TestMethod]
    public async Task ReturnsWhenWaitingSuccessful() {
        Daemon.SetDebugMode(true);
        var client = CreateClient();
        SetRuntimeConfiguration(
            servicesConfig: new Dictionary<IReadOnlyDescriptor, object> {
                {
                    Constants.ServiceDescriptor,
                    new MessageBrokerConfig {
                        HousekeepingDaemon = new MessageBrokerConfig.HousekeepingDaemonType {
                            Enable = false
                        },
                        DeliveryDaemon = new MessageBrokerConfig.DeliveryDaemonType {
                            MaxConcurrentMessages = 1,
                            DefaultDeliveryFailureDelaysToRetryMs = new int[] { 1000 },
                            DefaultMaxConsecutiveFailuresPerQueue = 3,
                            DefaultMaxItemRetries = 2
                        }
                    }
                }
            }
        );

        Svc3.Command2FailCount = 0;
        var msg = new TestPayload {
            SomeValueGuid = Guid.NewGuid()
        };
        var messageId = Guid.NewGuid();
        var correlationId = Identifiers.GenerateIdentifier();
        (await client.IssueCommand(new FabricMessage {
            MessageId = messageId,
            JsonPayload = msg.ToJson(),
            MessageTypeName = $"{nameof(TestCommands)}.{nameof(TestCommands.Command2)}",
            OriginatorCorrelationId = correlationId
        })).HasError.Should().BeFalse();

        DispatchDeliveryDaemonMessages();

        var result = await client.WaitOnMessage(new WaitOnMessageRequest {
            MessageId = messageId,
            WaitTimeoutSeconds = 2f
        });

        result.ResponseBody.Results.Length.Should().Be(1);
        result.ResponseBody.Complete.Should().BeTrue();
        result.ResponseBody.Results[0].HasError.Should().BeFalse();
    }

    [TestMethod]
    public async Task ReturnsWhenWaitingNotSuccessful() {
        Daemon.SetDebugMode(true);
        var client = CreateClient();
        SetRuntimeConfiguration(
            servicesConfig: new Dictionary<IReadOnlyDescriptor, object> {
                {
                    Constants.ServiceDescriptor,
                    new MessageBrokerConfig {
                        HousekeepingDaemon = new MessageBrokerConfig.HousekeepingDaemonType {
                            Enable = false
                        },
                        DeliveryDaemon = new MessageBrokerConfig.DeliveryDaemonType {
                            MaxConcurrentMessages = 1,
                            DefaultDeliveryFailureDelaysToRetryMs = new int[] { 1000 },
                            DefaultMaxConsecutiveFailuresPerQueue = 3,
                            DefaultMaxItemRetries = 2
                        }
                    }
                }
            }
        );

        Svc3.Command2FailCount = 1;
        var msg = new TestPayload {
            SomeValueGuid = Guid.NewGuid()
        };
        var messageId = Guid.NewGuid();
        var correlationId = Identifiers.GenerateIdentifier();
        (await client.IssueCommand(new FabricMessage {
            MessageId = messageId,
            JsonPayload = msg.ToJson(),
            MessageTypeName = $"{nameof(TestCommands)}.{nameof(TestCommands.Command2)}",
            OriginatorCorrelationId = correlationId
        })).HasError.Should().BeFalse();

        DispatchDeliveryDaemonMessages(1);

        var result = await client.WaitOnMessage(new WaitOnMessageRequest {
            MessageId = messageId,
            WaitTimeoutSeconds = 2f
        });

        result.ResponseBody.Results.Length.Should().Be(1);
        result.ResponseBody.Complete.Should().BeFalse();
        result.ResponseBody.Results[0].HasError.Should().BeTrue();
    }
}

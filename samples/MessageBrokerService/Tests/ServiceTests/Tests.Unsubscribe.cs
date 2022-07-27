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
public class Unsubscribe : TestBase
{
    [ClassInitialize]
    public static void ClassInitialize(TestContext _) { 
        TestBase.InitTests(); 
        SetRuntimeConfiguration(
            servicesConfig: new Dictionary<IReadOnlyDescriptor, object> {
                {
                    Constants.ServiceDescriptor,
                    new MessageBrokerConfig {
                        DeliveryDaemon = new MessageBrokerConfig.DeliveryDaemonType {
                            MaxConcurrentMessages = 1
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
    public async Task UnsubscribedServicesDoNotReceiveMessages() {
        var client = CreateClient();
        
        var subscriptions = new [] {
            new Subscription {
                Recipient = TestServices.Constants.Service4.Clone(),
                MessageTypeName = $"{nameof(TestCommands)}.{nameof(TestCommands.Command1)}"
            }
        };
        await client.Subscribe(new SubscribeRequest {
            Subscriptions = subscriptions
        });
        DispatchDeliveryDaemonMessages();

        var msg = new TestPayload {
            SomeValueGuid = Guid.NewGuid()
        };
        await client.IssueCommand(new FabricMessage {
            MessageId = Guid.NewGuid(),
            JsonPayload = msg.ToJson(),
            MessageTypeName = $"{nameof(TestCommands)}.{nameof(TestCommands.Command1)}",
            OriginatorCorrelationId = Identifiers.GenerateIdentifier()
        });
        DispatchDeliveryDaemonMessages();
        
        // verify that svc2 got it, proving that it was sent
        Svc2.TestValue.Should().Be(msg.SomeValueGuid);
        Svc2.MessageName.Should().Be("Command1");
        // verify that svc4 got it, proving the subscribe was successful
        Svc4.TestValue.Should().Be(msg.SomeValueGuid);
        Svc4.MessageName.Should().Be("Command1");

        var result = await client.Unsubscribe(new UnsubscribeRequest {
            Subscriptions = subscriptions
        });
        result.HasError.Should().BeFalse();
        DispatchDeliveryDaemonMessages();

        var originalGuid = msg.SomeValueGuid;
        msg.SomeValueGuid = Guid.NewGuid();

        await client.IssueCommand(new FabricMessage {
            MessageId = Guid.NewGuid(),
            JsonPayload = msg.ToJson(),
            MessageTypeName = $"{nameof(TestCommands)}.{nameof(TestCommands.Command1)}",
            OriginatorCorrelationId = Identifiers.GenerateIdentifier()
        });
        DispatchDeliveryDaemonMessages();

        // verify that svc2 got it, proving that it was sent
        Svc2.TestValue.Should().Be(msg.SomeValueGuid);
        Svc2.MessageName.Should().Be("Command1");
        // svc4 should NOT have the new message, proving the unsubscribe was successful
        Svc4.TestValue.Should().Be(originalGuid);
    }
}

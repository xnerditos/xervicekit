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
public class RaiseEvent : TestBase
{
    [ClassInitialize]
    public static void ClassInitialize(TestContext _) { 
        TestBase.InitTests(); 
        SetRuntimeConfiguration(
            servicesConfig: new Dictionary<IReadOnlyDescriptor, object> {
                {
                    Samples.MessageBroker.Constants.ServiceDescriptor,
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
    public async Task SubscriberReceivesEvent() {
        var client = CreateClient();

        var msg = new TestPayload {
            SomeValueGuid = Guid.NewGuid()
        };

        var messageId = Guid.NewGuid();
        var correlationId = Identifiers.GenerateIdentifier();
        var result = await client.RaiseEvent(new FabricMessage {
            MessageId = messageId,
            JsonPayload = msg.ToJson(),
            MessageTypeName = $"{nameof(TestEvents)}.{nameof(TestEvents.Event2)}",
            OriginatorCorrelationId = correlationId
        });
        result.HasError.Should().BeFalse();
        DispatchDeliveryDaemonMessages();
        Svc1.TestValue.Should().Be(msg.SomeValueGuid);
        Svc1.MessageName.Should().Be("Event2");
        Svc2.TestValue.Should().Be(msg.SomeValueGuid);
        Svc2.MessageName.Should().Be("Event2");
    }
}

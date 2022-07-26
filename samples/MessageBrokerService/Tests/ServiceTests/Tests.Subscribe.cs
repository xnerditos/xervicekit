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
public class Subscribe : TestBase
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
    public async Task SubscribedServicesReceivesMessages() {
        var client = CreateClient();
        
        var result = await client.Subscribe(new SubscribeRequest {
            Subscriptions = new [] {
                new Subscription {
                    Recipient = TestServices.Constants.Service4.Clone(),
                    MessageTypeName = $"{nameof(TestCommands)}.{nameof(TestCommands.Command1)}"
                }
            }
        });
        DispatchDeliveryDaemonMessages();
                    
        result.HasError.Should().BeFalse();
        
        var msg = new TestPayload {
            SomeValueGuid = Guid.NewGuid()
        };
        await client.IssueCommand(new FabricMessage {
            MessageId = Guid.NewGuid(),
            JsonPayload = msg.ToJson(),
            MessageTypeName = $"{nameof(TestCommands)}.{nameof(TestCommands.Command1)}",
            OriginatorCorrelationId = Identifiers.GenerateIdentifier()
        });
        result.HasError.Should().BeFalse();

        DispatchDeliveryDaemonMessages();
        Svc2.TestValue.Should().Be(msg.SomeValueGuid);
        Svc2.MessageName.Should().Be("Command1");
        // the service that just subscribed should have gotten the message
        Svc4.TestValue.Should().Be(msg.SomeValueGuid);
        Svc4.MessageName.Should().Be("Command1");
    }
}

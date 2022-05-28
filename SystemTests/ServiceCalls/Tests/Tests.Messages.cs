using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SystemTests.ServiceCalls.SvcSendsMessages;
using SystemTests.ServiceCalls.SvcSendsMessages.Entities;
using XKit.Lib.Testing;

namespace SystemTests.ServiceCalls.Tests {

    [TestClass]
    public class Messages : TestBase {

        [TestInitialize]
        public void Initialize() { TestBase.ClassInit(); }

        [TestCleanup]
        public void Teardown() { TestBase.ClassTeardown(); }

        [TestMethod]
        public async Task ServiceRaisesEvent() => await TestHostHelper.RunTestAsync(async () => {
            
            var client = CreateClient(SvcSendsMessages.Client.SvcSendsMessagesClientFactory.Factory);
            
            string testValue = Guid.NewGuid().ToString();
            var result = await client.RaisesEvent1(new Message {
                TheValue = testValue
            });
            
            result.ImmediateSuccess.Should().BeTrue();
            TestMessageBrokerService.WasMessageSent<TestEvents>(nameof(TestEvents.Event1)).Should().BeTrue();
        });

        [TestMethod]
        public async Task ServiceIssuesCommand() => await TestHostHelper.RunTestAsync(async () => {
            
            var client = CreateClient(SvcSendsMessages.Client.SvcSendsMessagesClientFactory.Factory);
            
            string testValue = Guid.NewGuid().ToString();
            var result = await client.IssuesCommand1(new Message {
                TheValue = testValue
            });
            
            result.ImmediateSuccess.Should().BeTrue();
            TestMessageBrokerService.WasMessageSent<TestCommands>(nameof(TestCommands.Command1)).Should().BeTrue();
        });

        [TestMethod]
        public async Task SubscriberReceivesEvent() => await TestHostHelper.RunTestAsync(async () => {
            
            string testValue = Guid.NewGuid().ToString();

            Guid messageId = Guid.NewGuid();
            await TestMessageBrokerService.SendMessage<TestEvents, Message>(
                nameof(TestEvents.Event1),
                new Message {
                    TheValue = testValue
                },
                messageId: messageId
            );
            
            MessageListeningService.TestValue.Should().Be(testValue);
        });

        [TestMethod]
        public async Task SubscriberReceivesCommand() => await TestHostHelper.RunTestAsync(async () => {
            
            string testValue = Guid.NewGuid().ToString();

            Guid messageId = Guid.NewGuid();
            await TestMessageBrokerService.SendMessage<TestCommands, Message>(
                nameof(TestCommands.Command1),
                new Message {
                    TheValue = testValue
                },
                messageId: messageId
            );
            
            MessageListeningService.TestValue.Should().Be(testValue);
        });
    }
}

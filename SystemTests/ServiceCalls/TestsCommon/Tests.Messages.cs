using System;
using System.Threading.Tasks;
using FluentAssertions;
using TestServices.SvcSendsMessages;
using XKit.Lib.Testing;

namespace SystemTests.ServiceCalls.TestsCommon {

    public class Messages : TestBase {

        public async Task Test_ServiceRaisesEvent() => await TestHelper.RunTestAsync(async () => {
            
            var client = CreateClient(SvcSendsMessagesClientFactory.Factory);
            
            string testValue = Guid.NewGuid().ToString();
            var result = await client.RaisesEvent1(new Message {
                TheValue = testValue
            });
            
            result.ImmediateSuccess.Should().BeTrue();
            TestMessageBrokerService.WasMessageSent<TestEvents>(nameof(TestEvents.Event1)).Should().BeTrue();
        });

        public async Task Test_ServiceIssuesCommand() => await TestHelper.RunTestAsync(async () => {
            
            var client = CreateClient(SvcSendsMessagesClientFactory.Factory);
            
            string testValue = Guid.NewGuid().ToString();
            var result = await client.IssuesCommand1(new Message {
                TheValue = testValue
            });
            
            result.ImmediateSuccess.Should().BeTrue();
            TestMessageBrokerService.WasMessageSent<TestCommands>(nameof(TestCommands.Command1)).Should().BeTrue();
        });

        public async Task Test_SubscriberReceivesEvent() => await TestHelper.RunTestAsync(async () => {
            
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

        public async Task Test_SubscriberReceivesCommand() => await TestHelper.RunTestAsync(async () => {
            
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

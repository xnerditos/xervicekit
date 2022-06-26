using XKit.Lib.Common.Host;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System;
using System.Linq;

namespace UnitTests.Daemons;

[TestClass]
public class DaemonEngine : DaemonTestBase {

    [TestInitialize]
    public void TestInitialize() => base.Init();

    [TestCleanup]
    public void TestCleanup() => base.Teardown();

    [TestMethod]
    public void StartsAndStops() {
        engine.RunState.Should().Be(DaemonRunStateEnum.Stopped);
        engine.Start();
        Yield();
        engine.RunState.Should().Be(DaemonRunStateEnum.Running);
        engine.Stop();
        Yield();
        engine.RunState.Should().Be(DaemonRunStateEnum.Stopped);
    }

    [TestMethod]
    public void TimerEventFiresPerDefaultTimeout() {
        var eventCount = 0;
        engine.EnableTimer = true;
        engine.DefaultTimerPeriodMilliseconds = 70;
        engine.OnTimerEvent = () => eventCount++;
        engine.Start();
        Yield();
        eventCount.Should().Be(1);
        Yield();
        eventCount.Should().Be(2);
        engine.Stop();
    }

    [TestMethod]
    public void TimerEventFiresPausesAndResumes() {
        var eventCount = 0;
        engine.OnTimerEvent = () => eventCount++;
        engine.Start();
        
        Yield();
        eventCount.Should().Be(0);
        
        engine.EnableTimer = true;
        Yield();
        eventCount.Should().Be(1);

        engine.Pause();
        Yield(200);
        eventCount.Should().Be(1);

        engine.Resume();
        Yield();
        eventCount.Should().Be(2);

        engine.Stop();        
    }

    [TestMethod]
    public void TimerEventFiresPerCalculatedTimeout() {
        var eventCount = 0;
        engine.EnableTimer = true;
        engine.DefaultTimerPeriodMilliseconds = 500;
        engine.OnDetermineTimerPeriod = () => 70;
        engine.OnTimerEvent = () => eventCount++;
        engine.Start();
        Yield();
        eventCount.Should().Be(1);
        Yield();
        eventCount.Should().Be(2);
        engine.Stop();
    }

    [TestMethod]
    public void TimerEventEnablesAndDisables() {
        var eventCount = 0;
        engine.OnTimerEvent = () => eventCount++;
        engine.Start();
        
        Yield();
        eventCount.Should().Be(0);
        
        engine.EnableTimer = true;
        Yield();
        eventCount.Should().Be(1);

        Yield();
        eventCount.Should().Be(2);

        engine.EnableTimer = false;
        Yield(200);
        eventCount.Should().Be(2);

        engine.EnableTimer = true;
        Yield();
        eventCount.Should().Be(3);

        engine.Stop();        
        Yield();
        eventCount.Should().Be(3);
    }

    [TestMethod]
    public void ProcessesPostedMessagesAsync() {

        engine.OnProcessMessage = 
            (Guid messageProcessingId, TestMessage message) => message.WasProcessed = true;

        var messages = new TestMessage[] {
            new(),
            new(),
            new(),
            new(),
        };

        engine.Start();
        engine.PostMessages(messages, triggerProcessing: false);

        Yield();
        messages.Where(m => m.WasProcessed).Count().Should().Be(0);

        engine.ProcessMessages(background: true).Should().BeTrue();
        Yield(1000);
        messages.Where(m => m.WasProcessed).Count().Should().Be(4);

        engine.Stop();
    }

    [TestMethod]
    public void ProcessesPostedMessagesSync() {

        engine.OnProcessMessage = 
            (Guid messageProcessingId, TestMessage message) => message.WasProcessed = true;

        var messages = new TestMessage[] {
            new(),
            new(),
            new(),
            new(),
        };

        engine.Start();
        engine.PostMessages(messages, triggerProcessing: false);

        Yield();
        messages.Should().AllSatisfy(m => m.WasProcessed.Should().BeFalse());

        engine.ProcessMessages(background: false).Should().BeTrue();
        Yield(200);
        messages.Should().AllSatisfy(m => m.WasProcessed.Should().BeTrue());

        engine.Stop();
    }

    [TestMethod]
    public void ProcessesPostedMessagesWhenAutoTriggered() {

        engine.OnProcessMessage = 
            (Guid messageProcessingId, TestMessage message) => message.WasProcessed = true;

        var messages = new TestMessage[] {
            new(),
            new(),
            new(),
            new(),
        };

        engine.Start();
        engine.PostMessages(messages, triggerProcessing: true);
        Yield();
        messages.Should().AllSatisfy(m => m.WasProcessed.Should().BeTrue());

        engine.Stop();
    }

    [TestMethod]
    public void ProcessesMessagesOneAtATimeSync() {

        engine.OnProcessMessage = 
            (Guid messageProcessingId, TestMessage message) => message.WasProcessed = true;

        var messages = new TestMessage[] {
            new(),
            new(),
            new(),
            new(),
        };

        engine.Start();
        engine.PostMessages(messages, triggerProcessing: false);

        engine.ProcessOneMessageSync().Should().BeTrue();
        messages.Where(m => m.WasProcessed).Count().Should().Be(1);
        engine.ProcessOneMessageSync().Should().BeTrue();
        messages.Where(m => m.WasProcessed).Count().Should().Be(2);
        engine.ProcessOneMessageSync().Should().BeTrue();
        messages.Where(m => m.WasProcessed).Count().Should().Be(3);
        engine.ProcessOneMessageSync().Should().BeTrue();
        messages.Where(m => m.WasProcessed).Count().Should().Be(4);
        engine.ProcessOneMessageSync().Should().BeFalse();

        engine.Stop();
    }

    [TestMethod]
    public void RespectsMaximumConcurrentProcessing() {

        engine.OnProcessMessage = (Guid messageProcessingId, TestMessage message) => {
            Thread.Sleep(90);    
            message.WasProcessed = true;
        };

        var messages = new TestMessage[] {
            new(),
            new(),
            new(),
            new(),
        };

        engine.MaxConcurrentMessages = 2;
        engine.Start();
        engine.PostMessages(messages, triggerProcessing: false);
        engine.ProcessMessages(background: true).Should().BeTrue();
        messages.Where(m => m.WasProcessed).Count().Should().Be(0);
        Yield();

        messages.Where(m => m.WasProcessed).Count().Should().Be(2);
        Yield();
        engine.Stop();
    }

    [TestMethod]
    public void FlushesProcessingMessagesBeforePausing() {
        engine.OnProcessMessage = (Guid messageProcessingId, TestMessage message) => {
            Thread.Sleep(90);    
            message.WasProcessed = true;
        };

        var messages = new TestMessage[] {
            new(),
            new(),
            new(),
            new(),
        };

        engine.MaxConcurrentMessages = 2;
        engine.Start();
        engine.PostMessages(messages, triggerProcessing: false);
        engine.ProcessMessages(background: true).Should().BeTrue();
        engine.Pause();

        Yield();
        engine.RunState.Should().Be(DaemonRunStateEnum.Pausing);
        messages.Where(m => m.WasProcessed).Count().Should().Be(2);

        Yield();
        messages.Where(m => m.WasProcessed).Count().Should().Be(4);
        engine.RunState.Should().Be(DaemonRunStateEnum.Paused);

        engine.Stop();
    }

    [TestMethod]
    public void FlushesProcessingMessagesBeforeStopping() {
        engine.OnProcessMessage = (Guid messageProcessingId, TestMessage message) => {
            Thread.Sleep(90);    
            message.WasProcessed = true;
        };

        var messages = new TestMessage[] {
            new(),
            new(),
            new(),
            new(),
        };

        engine.MaxConcurrentMessages = 2;
        engine.Start();
        engine.PostMessages(messages, triggerProcessing: false);
        engine.ProcessMessages(background: true).Should().BeTrue();
        engine.Stop();

        Yield();
        engine.RunState.Should().Be(DaemonRunStateEnum.Stopping);
        messages.Where(m => m.WasProcessed).Count().Should().Be(2);

        Yield();
        messages.Where(m => m.WasProcessed).Count().Should().Be(4);
        engine.RunState.Should().Be(DaemonRunStateEnum.Stopped);
    }

    [TestMethod]
    public void BeginsAndEndsBatchesWhenProcessingAsync() {
        var batchStartCount = 0;
        var batchEndCount = 0;

        engine.OnProcessMessage = (Guid messageProcessingId, TestMessage message) => {
            Thread.Sleep(90);    
            message.WasProcessed = true;
        };
        engine.OnStartProcessMessageBatch = () => batchStartCount++;
        engine.OnEndProcessMessageBatch = () => batchEndCount++;
        engine.MaxConcurrentMessages = 2;

        var messages = new TestMessage[] {
            new(),
            new(),
            new(),
            new(),
        };

        engine.Start();
        engine.PostMessages(messages, triggerProcessing: false);
        batchStartCount.Should().Be(0);
        batchEndCount.Should().Be(0);

        engine.ProcessMessages(background: true).Should().BeTrue();
        Yield();
        batchStartCount.Should().Be(1);
        messages.Where(m => m.WasProcessed).Count().Should().Be(2);
        batchEndCount.Should().Be(0);
        
        Yield();
        batchStartCount.Should().Be(1);
        messages.Where(m => m.WasProcessed).Count().Should().Be(4);
        batchEndCount.Should().Be(1);

        engine.Stop();
    }

    [TestMethod]
    public void BeginsAndEndsBatchesWhenProcessingSync() {
        var batchStartCount = 0;
        var batchEndCount = 0;

        engine.OnProcessMessage = 
            (Guid messageProcessingId, TestMessage message) => message.WasProcessed = true;
        engine.OnStartProcessMessageBatch = () => batchStartCount++;
        engine.OnEndProcessMessageBatch = () => batchEndCount++;

        var messages = new TestMessage[] {
            new(),
            new(),
            new(),
            new(),
        };

        engine.Start();
        engine.PostMessages(messages, triggerProcessing: false);
        batchStartCount.Should().Be(0);
        batchEndCount.Should().Be(0);

        engine.ProcessMessages(background: false).Should().BeTrue();
        batchStartCount.Should().Be(1);
        messages.Where(m => m.WasProcessed).Count().Should().Be(4);
        batchEndCount.Should().Be(1);

        engine.Stop();
    }

    [TestMethod]
    public void BeginsAndEndsBatchesWhenProcessingOneAtATime() {
        var batchStartCount = 0;
        var batchEndCount = 0;

        engine.OnProcessMessage = 
            (Guid messageProcessingId, TestMessage message) => message.WasProcessed = true;
        engine.OnStartProcessMessageBatch = () => batchStartCount++;
        engine.OnEndProcessMessageBatch = () => batchEndCount++;

        var messages = new TestMessage[] {
            new(),
            new(),
            new(),
            new(),
        };

        engine.Start();
        engine.PostMessages(messages, triggerProcessing: false);
        batchStartCount.Should().Be(0);
        batchEndCount.Should().Be(0);

        engine.ProcessOneMessageSync().Should().BeTrue();
        messages.Where(m => m.WasProcessed).Count().Should().Be(1);
        batchStartCount.Should().Be(1);
        batchEndCount.Should().Be(0);

        engine.ProcessOneMessageSync().Should().BeTrue();
        messages.Where(m => m.WasProcessed).Count().Should().Be(2);
        batchStartCount.Should().Be(1);
        batchEndCount.Should().Be(0);

        engine.ProcessOneMessageSync().Should().BeTrue();
        messages.Where(m => m.WasProcessed).Count().Should().Be(3);
        batchStartCount.Should().Be(1);
        batchEndCount.Should().Be(0);

        engine.ProcessOneMessageSync().Should().BeTrue();
        messages.Where(m => m.WasProcessed).Count().Should().Be(4);
        batchStartCount.Should().Be(1);
        batchEndCount.Should().Be(1);

        engine.Stop();
    }
}

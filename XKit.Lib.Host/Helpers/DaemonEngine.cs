using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using XKit.Lib.Common.Host;

namespace XKit.Lib.Host.Helpers {

    public sealed class DaemonEngine<TMessage> : IDaemonEngine<TMessage>
        where TMessage : class {

        private const int SmallSleepThreadDelayMilliseconds = 100;
        private volatile DaemonRunStateEnum runState = DaemonRunStateEnum.Stopped;
        private Thread messageThread;
        private ManualResetEvent messageThreadSignal = new ManualResetEvent(false);
        private volatile int messageThreadWakeDelayMillisecondsNoWorkWaiting = -1;
        private volatile int messageThreadWakeDelayMillisecondsWorkIsWaiting = 1000;
        private volatile int messageThreadMillisecondsStopTimeout = 15000;
        private volatile int maxConcurrentMessages = 4;
        private volatile bool autoPulseActive = true;
        private volatile bool messageDispatchActive = true;
        private volatile bool debugMode = false;
        private volatile bool enableEnqueueEvent = false;
        private DateTime? nextEnqueueMessagesEvent = null;

        private ConcurrentQueue<TMessage> waitingMessages = new ConcurrentQueue<TMessage>();
        private ConcurrentDictionary<Guid, TMessage> processingMessages = new ConcurrentDictionary<Guid, TMessage>();
        private OnDaemonEventDelegate onStartup;
        private OnDaemonEventPulseDelegate onPulse;
        private OnDaemonEventDelegate onStartDispatch;
        private OnDaemonEventDispatchMessageDelegate<TMessage> onDispatchMessage;
        private OnDaemonEventDelegate onEndDispatch;
        private OnDaemonEventDelegate onTeardown;
        private OnDaemonEventDelegate onEnqueueMessagesTimer;
        private OnDaemonEventDetermineEnqueueMessagesTimerPeriodDelegate onDetermineEnqueueMessagesTimerPeriod;
        
        public DaemonEngine() {
            messageThread = new Thread(MainMessagePump);
        }

        // =====================================================================
        // IDaemon
        // =====================================================================
        
        DaemonRunStateEnum IDaemonEngine.RunState => runState;

        void IDaemonEngine.Pause() {
            if (this.runState == DaemonRunStateEnum.Running) {
                this.runState = DaemonRunStateEnum.Pausing;
                TriggerPulse();
            } 
        }

        void IDaemonEngine.Resume() {
            if (this.runState == DaemonRunStateEnum.Paused) {
                this.runState = DaemonRunStateEnum.Resuming;
                TriggerPulse();
            } 
        }

        void IDaemonEngine.Pulse() {
            TriggerPulse();
        }

        void IDaemonEngine<TMessage>.PostMessage(TMessage message, bool triggerPulse) {
            if (this.runState == DaemonRunStateEnum.Stopping) {
                throw new Exception("Cannot post messages while the daemon is stopping");
            }
            this.waitingMessages.Enqueue(message);
            if (triggerPulse) { TriggerPulse(); }
        }

        // Guid IDaemonEngine<TMessage>.ProcessMessageDirectly(TMessage message) {
        //     return this.RunDispatchOneMessage(message);            
        // }

        bool IDaemonEngine.DispatchMessageDirectly() 
            => RunDispatchOneMessage() != null;

        void IDaemonEngine.Start() {
            StartMainThread(); 
        }

        void IDaemonEngine.Stop() {
            StopMainThread();
        }

        bool IDaemonEngine.IsAutomaticMessageDispatchActive => this.messageDispatchActive && !this.debugMode;

        void IDaemonEngine.SuspendAutomaticMessageDispatch() {
            this.messageDispatchActive = false;
        }

        void IDaemonEngine.ResumeAutomaticMessageDispatch() {
            this.messageDispatchActive = true;        
        }

        void IDaemonEngine.SignalFinishMessageProcessing(Guid messageProcessingId) {
            TMessage message;
            processingMessages.TryRemove(messageProcessingId, out message);
        }

        int IDaemonEngine.WakeDelayMillisecondsNoWork { 
            get => this.messageThreadWakeDelayMillisecondsNoWorkWaiting; 
            set => this.messageThreadWakeDelayMillisecondsNoWorkWaiting = value; 
        }

        int IDaemonEngine.TimeoutToStopMilliseconds { 
            get => this.messageThreadMillisecondsStopTimeout; 
            set => this.messageThreadMillisecondsStopTimeout = value; 
        }

        int IDaemonEngine.WakeDelayMillisecondsWorkWaiting {
            get => this.messageThreadWakeDelayMillisecondsWorkIsWaiting;
            set => this.messageThreadWakeDelayMillisecondsWorkIsWaiting = value;
        }

        int IDaemonEngine.MaxConcurrentMessages {
            get => this.maxConcurrentMessages;
            set => this.maxConcurrentMessages = value;
        }

        bool IDaemonEngine.AutoPulseActive { 
            get => this.autoPulseActive;
            set {
                this.autoPulseActive = value;
                if (value && this.runState == DaemonRunStateEnum.Running) {
                    TriggerPulse();
                }
            }
        }

        bool IDaemonEngine.EnableEnqueueEvent { 
            get => enableEnqueueEvent; 
            set {                
                enableEnqueueEvent = value; 

                // if the next Enqueue event has passed, set it to this so that
                // the actual wake delay will just be the normal delay timeouts.
                this.nextEnqueueMessagesEvent = null;

                if (value && this.runState == DaemonRunStateEnum.Running) {
                    TriggerPulse();
                }
            }
        }

        int IDaemonEngine.ActiveMessageCount => this.processingMessages.Count;
        int IDaemonEngine.WaitingMessageCount => this.waitingMessages.Count;
        bool IDaemonEngine.DebugMode {
            get => this.debugMode;
            set {
                this.debugMode = value;
                if (!value && this.runState == DaemonRunStateEnum.Running) {
                    TriggerPulse();
                }
            }
        }
        
        bool IDaemonEngine.HasMessages => this.waitingMessages.Count > 0 || this.processingMessages.Count > 0;

        OnDaemonEventDelegate IDaemonEngine.OnStartup {
            get => onStartup;
            set => onStartup = value;
        }

        OnDaemonEventPulseDelegate IDaemonEngine.OnPulse {
            get => onPulse;
            set => onPulse = value;
        }

        OnDaemonEventDelegate IDaemonEngine.OnStartDispatch {
            get => onStartDispatch;
            set => onStartDispatch = value;
        }

        OnDaemonEventDispatchMessageDelegate<TMessage> IDaemonEngine<TMessage>.OnDispatchMessage {
            get => onDispatchMessage;
            set => onDispatchMessage = value;
        }

        OnDaemonEventDelegate IDaemonEngine.OnEndDispatch {
            get => onEndDispatch;
            set => onEndDispatch = value;
        }

        OnDaemonEventDelegate IDaemonEngine.OnTeardown { 
            get => onTeardown;
            set => onTeardown = value;
        }

        OnDaemonEventDelegate IDaemonEngine.OnEnqueueMessagesTimer {
            get => onEnqueueMessagesTimer;
            set => onEnqueueMessagesTimer = value;
        }

        OnDaemonEventDetermineEnqueueMessagesTimerPeriodDelegate IDaemonEngine.OnDetermineEnqueueMessagesTimerPeriod { 
            get => onDetermineEnqueueMessagesTimerPeriod; 
            set => onDetermineEnqueueMessagesTimerPeriod = value; 
        }

        // =====================================================================
        // main thread work control
        // =====================================================================

        void TriggerPulse() {
            messageThreadSignal.Set();
        }

        private void MainMessagePump() {
            
            while(true) {
                
                switch(runState) {
                    
                    case DaemonRunStateEnum.Starting:
                        onStartup?.Invoke();
                        runState = DaemonRunStateEnum.Running;
                        break;
                    
                    case DaemonRunStateEnum.Running:
                        CheckRunEnqueueMessagesEvent();
                        DispatchMessagesAndSleepTilPulse();
                        break;
                    
                    case DaemonRunStateEnum.Pausing:
                        SleepWhilePaused();
                        break;
                    
                    case DaemonRunStateEnum.Stopping:
                        
                        if (waitingMessages.Any()) {
                            // need to clear the queue
                            DispatchMessagesAndSleepTilPulse();
                        } else if (processingMessages.Any()) {
                            // need to wait for currently processing to stop
                            SleepWaitForSignal(wakeOnDelay: true);
                        } else {
                            // all clear, tear it down
                            onTeardown?.Invoke();
                            runState = DaemonRunStateEnum.Stopped;
                            return;
                        }
                        break;

                    default:
                        throw new Exception("Unexpected daemon run state");
                }
            }
        }

        private void SleepWhilePaused() {

            runState = DaemonRunStateEnum.Paused;

            do {
                SleepWaitForSignal(wakeOnDelay: false);
            } while(runState == DaemonRunStateEnum.Paused);

            // transition state
            switch (runState) {
                case DaemonRunStateEnum.Resuming: 
                    runState = DaemonRunStateEnum.Running;
                    break;
            }
        }

        private void CheckRunEnqueueMessagesEvent() {
            if (enableEnqueueEvent &&
                messageDispatchActive &&
                !debugMode &&
                processingMessages.Count < maxConcurrentMessages
            ) {

                if (!this.nextEnqueueMessagesEvent.HasValue) {
                    this.nextEnqueueMessagesEvent = CalculateTimeNextEnqueueMessageEvent();
                } 

                if (DateTime.UtcNow >= nextEnqueueMessagesEvent.GetValueOrDefault(DateTime.MaxValue)) {
                    onEnqueueMessagesTimer.Invoke();
                    this.nextEnqueueMessagesEvent = CalculateTimeNextEnqueueMessageEvent();
                }
            } 
        }

        private DateTime? CalculateTimeNextEnqueueMessageEvent() {
            
            var delayForNextEvent = onDetermineEnqueueMessagesTimerPeriod?.Invoke();
            if (delayForNextEvent.HasValue) {
                return DateTime.UtcNow.Add(delayForNextEvent.Value);
            } else {
                return null;
            }
        }

        private void DispatchMessagesAndSleepTilPulse() {
            bool canDispatch() => 
                messageDispatchActive &&
                !debugMode &&
                (this.runState == DaemonRunStateEnum.Running || this.runState == DaemonRunStateEnum.Stopping) &&
                processingMessages.Count < maxConcurrentMessages && 
                waitingMessages.Count > 0;

            if (canDispatch()) {
                onStartDispatch?.Invoke();
                do {
                    RunDispatchOneMessage();
                } while(canDispatch());
                onEndDispatch?.Invoke();
            }

            var manualPulse = SleepWaitForSignal(wakeOnDelay: true);
            onPulse?.Invoke(manualPulse);
        }

        private Guid? RunDispatchOneMessage() {
            if (waitingMessages.TryDequeue(out var msg)) {
                return RunDispatchOneMessage(msg);
            }
            return null;
        }

        private Guid? RunDispatchOneMessage(TMessage msg) {
            Guid messageProcessingId = Guid.NewGuid();
            processingMessages[messageProcessingId] = msg;
            onDispatchMessage?.Invoke(messageProcessingId, msg);
            return messageProcessingId;
        }

        /// <summary>
        /// Wait for a signal event or timeout
        /// </summary>
        /// <param name="wakeOnDelay"></param>
        /// <returns>true if wake was from a signal, false if timeout</returns>
        private bool SleepWaitForSignal(bool wakeOnDelay) {
            int delay = int.MaxValue;
            if (wakeOnDelay) {
                if (this.waitingMessages.Any()) {
                    delay = messageThreadWakeDelayMillisecondsWorkIsWaiting;
                } else if (autoPulseActive) {
                    int millisecondsToNextEnqueueEvent = 
                        !nextEnqueueMessagesEvent.HasValue ? int.MaxValue :
                        (int)(nextEnqueueMessagesEvent.Value - DateTime.UtcNow).TotalMilliseconds;
                    delay = 
                        millisecondsToNextEnqueueEvent < messageThreadWakeDelayMillisecondsNoWorkWaiting ?
                        millisecondsToNextEnqueueEvent :
                        messageThreadWakeDelayMillisecondsNoWorkWaiting;
                }
            }
            
            return delay <= 0 ? false : messageThreadSignal.WaitOne(delay == int.MaxValue ? -1 : delay);    
        }

        // =====================================================================
        // base class worker methods and utility
        // =====================================================================

        private void StartMainThread() {
            this.runState = DaemonRunStateEnum.Starting;
            messageThread.Start();
        }

        private bool StopMainThread() {
            if (this.runState == DaemonRunStateEnum.Running || this.runState == DaemonRunStateEnum.Paused) {
                this.runState = DaemonRunStateEnum.Stopping;
            } 

            this.TriggerPulse();
            DateTime fromTime = DateTime.UtcNow;
            while(this.runState != DaemonRunStateEnum.Stopped && !passedTimeout()) {
                Thread.Sleep(SmallSleepThreadDelayMilliseconds);
            }

            return this.runState == DaemonRunStateEnum.Stopped;

            // -------------------------
            bool passedTimeout()
                => (int)DateTime.UtcNow.Subtract(fromTime).TotalMilliseconds > this.messageThreadMillisecondsStopTimeout;
        }
    }
}
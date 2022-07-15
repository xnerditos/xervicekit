using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Timers;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Utility.Extensions;
using XKit.Lib.Common.Utility.Threading;

namespace XKit.Lib.Host.Helpers {

    public sealed class DaemonEngine<TMessage> : IDaemonEngine<TMessage>
        where TMessage : class {

        private volatile DaemonRunStateEnum runState = DaemonRunStateEnum.Stopped;
        private System.Timers.Timer timer;
        private volatile uint defaultTimerPeriodMilliseconds = 60 * 1000; // default 1 one minute
        private volatile int maxConcurrentMessages = 4;
        private volatile bool debugMode = false;
        private volatile bool enableTimer = false;
        // NOTE:  Really not loving the idea of using a boolean flag to indicate
        //        that a batch is running.  This approach makes for more complicated logic. 
        //        Revisit this in the future.
        private volatile bool messageBatchRunning = false; 

        private readonly ConcurrentQueue<TMessage> waitingMessages = new();
        private readonly ConcurrentDictionary<Guid, TMessage> processingMessages = new();
        private readonly ConcurrentDictionary<Guid, Task> asyncWorkers = new();
        
        private OnDaemonEventDelegate onStartProcessMessageBatch;
        private OnDaemonEventProcessMessageDelegate<TMessage> onProcessMessage;
        private OnDaemonEventDelegate onEndProcessMessageBatch;
        private OnDaemonEventDelegate onTimerEvent;
        private OnDaemonEventDetermineTimerPeriodDelegate onDetermineTimerPeriod;
        
        public DaemonEngine() { }

        // =====================================================================
        // IDaemon
        // =====================================================================
        
        DaemonRunStateEnum IDaemonEngine.RunState => runState;

        void IDaemonEngine.Pause() {
            if (this.runState == DaemonRunStateEnum.Running) {
                this.runState = DaemonRunStateEnum.Pausing;
                StopTimerIfEnabled();
                if (asyncWorkers.IsEmpty) {
                    this.runState = DaemonRunStateEnum.Paused;
                }
                // NOTE:  IF there are background processing messages, then they will end up setting the state when they finish
            } 
        }

        void IDaemonEngine.Resume() {
            if (runState == DaemonRunStateEnum.Paused) {
                runState = DaemonRunStateEnum.Running;
                StartTimerIfEnabled();
            } 
        }

        void IDaemonEngine<TMessage>.PostMessage(TMessage message, bool triggerProcessing) => PostMessages(triggerProcessing, message);

        void IDaemonEngine<TMessage>.PostMessages(TMessage[] messages, bool triggerProcessing) => PostMessages(triggerProcessing, messages);

        bool IDaemonEngine.ProcessMessages(bool background) {
            return ProcessMessageBatch(background);
        } 

        void IDaemonEngine.Start() {
            if (runState == DaemonRunStateEnum.Stopped) {
                runState = DaemonRunStateEnum.Running;
                StartTimerIfEnabled();
            }
        }

        void IDaemonEngine.Stop() {
            if (runState != DaemonRunStateEnum.Stopping) {
                this.runState = DaemonRunStateEnum.Stopping;
                StopTimerIfEnabled();
                if (asyncWorkers.IsEmpty) {
                    this.runState = DaemonRunStateEnum.Stopped;
                }
                // NOTE:  IF there are background processing messages, then they will end up setting the state when they finish
            }
        }

        bool IDaemonEngine.ProcessOneMessageSync() {
            if (CanProcessMessages()) {
                ProcessBatchBeginIfNeeded();
                ProcessOneMessageSync();
                ProcessBatchEndIfReady(false);
                return true;
            }
            return false;
        }

        void IDaemonEngine.ManualFireTimerEvent() {
            ElapsedTimerEventHandler(null, null);            
        }

        int IDaemonEngine.MaxConcurrentMessages {
            get => this.maxConcurrentMessages;
            set => this.maxConcurrentMessages = value;
        }

        uint IDaemonEngine.DefaultTimerPeriodMilliseconds { 
            get => this.defaultTimerPeriodMilliseconds;
            set => this.defaultTimerPeriodMilliseconds = value;
        }

        bool IDaemonEngine.EnableTimer { 
            get => enableTimer; 
            set {                
                enableTimer = value; 
                if (value) {
                    StartTimerIfEnabled();
                } else {
                    StopTimerIfEnabled();
                }
            }
        }

        int IDaemonEngine.ActiveMessageCount => this.processingMessages.Count;
        int IDaemonEngine.WaitingMessageCount => this.waitingMessages.Count;
        bool IDaemonEngine.DebugMode {
            get => this.debugMode;
            set {
                this.debugMode = value;
                if (value) {
                    StartTimerIfEnabled();
                } else {
                    StopTimerIfEnabled();
                }
            }
        }
        
        bool IDaemonEngine.HasMessages => !waitingMessages.IsEmpty || !processingMessages.IsEmpty;

        OnDaemonEventDelegate IDaemonEngine.OnStartProcessMessageBatch {
            get => onStartProcessMessageBatch;
            set => onStartProcessMessageBatch = value;
        }

        OnDaemonEventProcessMessageDelegate<TMessage> IDaemonEngine<TMessage>.OnProcessMessage {
            get => onProcessMessage;
            set => onProcessMessage = value;
        }

        OnDaemonEventDelegate IDaemonEngine.OnEndProcessMessageBatch {
            get => onEndProcessMessageBatch;
            set => onEndProcessMessageBatch = value;
        }

        OnDaemonEventDelegate IDaemonEngine.OnTimerEvent {
            get => onTimerEvent;
            set => onTimerEvent = value;
        }

        OnDaemonEventDetermineTimerPeriodDelegate IDaemonEngine.OnDetermineTimerPeriod { 
            get => onDetermineTimerPeriod; 
            set => onDetermineTimerPeriod = value; 
        }

        // =====================================================================

        private void PostMessages(bool triggerProcessing, params TMessage[] messages) {
            if (runState != DaemonRunStateEnum.Running) {
                throw new Exception("Can only post messages while the daemon is running");
            }
            messages.ForEach(m => this.waitingMessages.Enqueue(m));
            if (triggerProcessing && !debugMode) {
                ProcessMessageBatch(background: true);
            }
        }

        private void StartTimerIfEnabled() {
            if (runState == DaemonRunStateEnum.Running && enableTimer && onTimerEvent != null) {
                if (this.timer == null) {
                    timer = new() {
                        AutoReset = false
                    };
                    timer.Elapsed += ElapsedTimerEventHandler;
                }
                this.timer.Interval = this.onDetermineTimerPeriod?.Invoke() ?? this.defaultTimerPeriodMilliseconds;
                if (!debugMode) {
                    timer.Start();
                }
            }
        }

        private void ElapsedTimerEventHandler(object sender, ElapsedEventArgs e) {
            if (runState == DaemonRunStateEnum.Running) {
                onTimerEvent?.Invoke();
                StartTimerIfEnabled();
            }
        }

        private void StopTimerIfEnabled() {
            timer?.Stop();
        }

        private bool CanProcessMessages() => 
            (this.runState == DaemonRunStateEnum.Running || this.runState == DaemonRunStateEnum.Stopping) &&
            !waitingMessages.IsEmpty;

        private bool ProcessMessageBatch(bool background) {
            if (debugMode) {
                return false;
            }

            if (CanProcessMessages()) {
                ProcessBatchBeginIfNeeded();
                if (!background) {
                    // if running synchronously, just run the worker method directly
                    ProcessMessageWorker(false);
                } else {
                    // the first thread will start the rest
                    StartOneBackgroundWorker();
                }
                return true;
            }
            return false;
        }

        private void StartOneBackgroundWorker() {

            var workerId = Guid.NewGuid();
            var task = TaskUtil.RunSyncAsAsync(() => {
                    ProcessMessageWorker(true);
                    asyncWorkers.TryRemove(workerId, out _);
                }, true);

            asyncWorkers[workerId] = task;
            task.Start();
            task.Forget();
        }

        private void ProcessMessageWorker(bool background) {

            Guid lastProcessedMessageId;
            do {
                if (background) {
                    ProcessCheckAddBackgroundWorkers();
                }
                lastProcessedMessageId = ProcessOneMessageSync();
            } while(lastProcessedMessageId != default);

            ProcessBatchEndIfReady(background);
        }

        private void ProcessCheckAddBackgroundWorkers() {
            if (debugMode) {
                return;
            }

            // to prevent multiple threads from trying to scale workers at once, 
            // lock on processingMessages.  Note that there is a reason we scale workers
            // inside of the workers.  If the max number of workers is running, and
            // work runs out so they start to finish, and during that time a new message comes
            // in, letting the existing workers scale up will bring the number back up again.
            lock (processingMessages) {
                while(CanProcessMessages() && asyncWorkers.Count < maxConcurrentMessages) {
                    StartOneBackgroundWorker();
                }
            }
        }

        private Guid ProcessOneMessageSync() {
            
            if (waitingMessages.TryDequeue(out var msg)) {
                Guid messageProcessingId = Guid.NewGuid();
                processingMessages[messageProcessingId] = msg;
                onProcessMessage?.Invoke(messageProcessingId, msg);
                processingMessages.TryRemove(messageProcessingId, out _);
                return messageProcessingId;
            }
            return default;
        }

        private void ProcessBatchBeginIfNeeded() {
            lock (waitingMessages) {
                if (!messageBatchRunning) {
                    messageBatchRunning = true;
                    onStartProcessMessageBatch?.Invoke();
                }
            }
        }

        private void ProcessBatchEndIfReady(bool background) {
            bool canEndBatch() => messageBatchRunning && processingMessages.IsEmpty && waitingMessages.IsEmpty;

            // to avoid unnecessarily locking, do a pre-check and then confirm
            // once we lock
            if (canEndBatch()) {
                lock (waitingMessages) {

                    // if there are no more processing messages, then we have reached the end of the batch.
                    if (canEndBatch()) { 
                        onEndProcessMessageBatch?.Invoke();
                        messageBatchRunning = false;
                        switch (runState) {
                            case DaemonRunStateEnum.Pausing:
                                runState = DaemonRunStateEnum.Paused;
                                break;
                            case DaemonRunStateEnum.Stopping:
                                runState = DaemonRunStateEnum.Stopped;
                                break;
                        }
                    } else {
                        // if running synchronously, we will get here quite often.  But when running async, it would be an edge
                        // case to get here where we finished a thread and yet messages are waiting.  Basically one snuck in while
                        // we were ending.  Give an opportunity to start new threads. 
                        if (background) {
                            ProcessCheckAddBackgroundWorkers();
                        }
                    }
                }
            }
        }
    }
}

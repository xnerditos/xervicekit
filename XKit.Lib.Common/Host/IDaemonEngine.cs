using System;

namespace XKit.Lib.Common.Host {

    public delegate void OnDaemonEventDelegate();
    public delegate void OnDaemonEventProcessMessageDelegate<TMessage>(Guid messageProcessingId, TMessage message) where TMessage : class;
    public delegate uint? OnDaemonEventDetermineTimerPeriodDelegate();  

    /// <summary>
    /// Interface to a daemon process.   
    /// </summary>
    /// <remarks>
    public interface IDaemonEngine {

        /// <summary>
        /// Run state of the daemon process
        /// </summary>
        DaemonRunStateEnum RunState { get; }

        /// <summary>
        /// <summary>
        /// Starts the daemon engine running
        /// </summary>
        void Start();

        /// <summary>
        /// Pauses the daemon engine.  Existing tasks (messages in process) will finish, but no new tasks or threads
        /// should be spawned while paused.
        /// </summary>
        /// <returns></returns>
        void Pause();

        /// <summary>
        /// Resumes the daemon engine from a paused state.  
        /// </summary>
        /// <returns></returns>
        void Resume();

        /// <summary>
        /// Stops the daemon
        /// </summary>
        /// <returns></returns>
        void Stop();

        /// <summary>
        /// CAuses messages to be processed / processed.  Returns false if no message available to process.
        /// <param name="count">The number of messages to attempt (max) to process</param>
        /// <param name="background">if true, messages are processed in threads and the current thread   
        /// If false, then messages are processed on the current thread.i</param>
        /// </summary>
        bool ProcessMessages(bool background = true);

        /// <summary>
        /// Processes a single message synchronously. Mostly used for debugging scenarios 
        /// </summary>
        /// <returns></returns>
        bool ProcessOneMessageSync();

        /// <summary>
        /// Manually fires the timer event.  Mostly used for debugging scenarios
        /// </summary>
        /// <returns></returns>
        void ManualFireTimerEvent();

        /// <summary>
        /// Sets when the next timer event will be.  Note that for the default implementation of 
        /// the daemon engine, this will reset any value previously determined by OnDetermineTimerPeriod 
        /// </summary>
        /// <param name="delay"></param>
        void SetTimerDelay(uint? delay);

        /// <summary>
        /// Controls maximum number of messages that will processed  
        /// to be processed concurrently if the daemon processes on the thread pool (async)
        /// </summary>
        /// <value></value>
        int MaxConcurrentMessages { get; set; }

        /// <summary>
        /// If enabled, the daemon will fire the events OnDetermineTimerPeriod and
        /// OnEnqueueMessagesTimer
        /// </summary>
        /// <value></value>
        bool EnableTimer { get; set; }

        bool DebugMode { get; set; }

        int ActiveMessageCount { get; }
        int WaitingMessageCount { get; }
        bool HasMessages { get; } 
        
        /// <summary>
        /// Returns a TimeSpan to indicate when the next OnEnqueueMessagesTimer Event should fire.  If the 
        /// delegate returns null or if there is no OnDetermineTimerPeriod supplied, then DefaultTimerPeriodMilliseconds
        /// will be used.
        /// </summary>
        /// <value></value>
        OnDaemonEventDetermineTimerPeriodDelegate OnDetermineTimerPeriod { get; set; }

        /// <summary>
        /// Event when a timer fires to allow for messages to get posted.  
        /// </summary>
        /// <value></value>
        OnDaemonEventDelegate OnTimerEvent { get; set; }

        /// <summary>
        /// Occurs at the start of processing messages.  This is a good place to provision resources
        /// that are needed for a block of work (as series of messages).  If the daemon should just process
        /// once per wake cycle, then drop one message in the queue and put the processing work in 
        /// `RunProcessMessage`
        /// </summary>
        OnDaemonEventDelegate OnStartProcessMessageBatch { get; set; }

        /// <summary>
        /// Occurs at the end of processing messages.  This is a good place to free resources
        /// that are needed for a block of work (as series of messages)
        /// </summary>
        OnDaemonEventDelegate OnEndProcessMessageBatch { get; set; }
    }

    public interface IDaemonEngine<TMessage> : IDaemonEngine where TMessage : class {

        /// <summary>
        /// Posts a message for the daemon message thread.  Note that a daemon may add messages to it's own
        /// queue as well, perhaps checking for some condition at each pulse.  
        /// </summary>
        /// <param name="message">message object to post</param>
        /// <param name="triggerProcessing">true if background message processing should be triggered immediately</param>
        void PostMessage(TMessage message, bool triggerProcessing = true);

        /// <summary>
        /// Posts a message for the daemon message thread.  Note that a daemon may add messages to it's own
        /// queue as well, perhaps checking for some condition at each pulse.  
        /// </summary>
        /// <param name="message">message object to post</param>
        /// <param name="triggerProcessing">true if background message processing should be triggered immediately</param>
        void PostMessages(TMessage[] message, bool triggerProcessing = true);

        /// <summary>
        /// Put the processing main work of handling messages here. 
        /// </summary>
        /// <param name="message"></param>
        OnDaemonEventProcessMessageDelegate<TMessage> OnProcessMessage { get; set; }
    }

    public enum DaemonRunStateEnum {

        /// <summary>
        /// Daemon is not running
        /// </summary>
        Stopped,

        /// <summary>
        /// Daemon is running normally
        /// </summary>
        Running,

        /// <summary>
        /// Daemon is in the process of pausing.  Existing work is finishing
        /// before reaching a paused state.
        /// </summary>
        Pausing,

        /// <summary>
        /// Daemon is paused.  Work is not being done, but no resources are still
        /// allocated.
        /// </summary>
        Paused,

        /// <summary>
        /// Daemon is not running. Existing work is finishing
        /// before reaching a stopped state.
        /// </summary>
        Stopping
    }
}

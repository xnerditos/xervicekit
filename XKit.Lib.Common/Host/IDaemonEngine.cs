using System;

namespace XKit.Lib.Common.Host {

    public delegate void OnDaemonEventDelegate();
    public delegate void OnDaemonEventPulseDelegate(bool manualPulse);
    public delegate void OnDaemonEventDispatchMessageDelegate<TMessage>(Guid messageProcessingId, TMessage message) where TMessage : class;
    public delegate TimeSpan? OnDaemonEventDetermineEnqueueMessagesTimerPeriodDelegate();  

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
        /// True if messages will be dispatched during a pulse 
        /// </summary>
        bool IsAutomaticMessageDispatchActive { get; }

        /// <summary>
        /// Starts the daemon engine running
        /// </summary>
        void Start();

        /// <summary>
        /// Pulses the message thread wake up
        /// </summary>
        void Pulse();

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
        /// Suspends the dispatch of messages when the message thread pulses
        /// </summary>
        /// <returns></returns>
        void SuspendAutomaticMessageDispatch();

        /// <summary>
        /// Resumes the dispatch of messages when the message thread pulses
        /// </summary>
        /// <returns></returns>
        void ResumeAutomaticMessageDispatch();

        /// <summary>
        /// Stops the daemon
        /// </summary>
        /// <returns></returns>
        void Stop();

        /// <summary>
        /// Call to indicate that the message has finished processing
        /// </summary>
        /// <param name="messageProcessingId"></param>
        void SignalFinishMessageProcessing(Guid messageProcessingId);

        /// <summary>
        /// Forces the dispatch of a message immedately on the current thread.  Returns false if 
        /// no message available to dispatch
        /// </summary>
        bool DispatchMessageDirectly();

        /// <summary>
        /// Set by the derived class to control the main message thread wake delay when
        /// no work is in the message queue at the time of the thread sleeping.
        /// </summary>
        /// <value></value>
        int WakeDelayMillisecondsNoWork { get; set; } 

        /// <summary>
        /// Set by the derived class to control the main message thread wake delay when
        /// no work is in the message queue at the time of the thread sleeping.
        /// </summary>
        /// <value></value>
        int TimeoutToStopMilliseconds { get; set; } 

        /// <summary>
        /// Controls the main message thread wake delay when
        /// work _is_ in the message queue at the time of the thread sleeping.
        /// </summary>
        /// <value></value>
        int WakeDelayMillisecondsWorkWaiting { get; set; }

        /// <summary>
        /// Controls maximum number of messages that will dispatched  
        /// to be processed concurrently if the daemon processes on the thread pool (async)
        /// </summary>
        /// <value></value>
        int MaxConcurrentMessages { get; set; }

        /// <summary>
        /// Allow the message thread to automatically pulse or not.
        /// </summary>
        /// <value></value>
        bool AutoPulseActive { get; set; } 

        /// <summary>
        /// If enabled, the daemon will fire the events OnDetermineEnqueueMessagesTimerPeriod and
        /// OnEnqueueMessagesTimer
        /// </summary>
        /// <value></value>
        bool EnableEnqueueEvent { get; set; }

        bool DebugMode { get; set; }

        int ActiveMessageCount { get; }
        int WaitingMessageCount { get; }
        bool HasMessages { get; } 
        
        /// <summary>
        /// General process startup should go here
        /// </summary>
        OnDaemonEventDelegate OnStartup { get; set; }

        /// <summary>
        /// Event when a pulse occurs.
        /// A pulse occurs in association with the message thread waking up, and can manual or can 
        /// occur as part of the normal wake/sleep cycle.
        /// </summary>
        /// <param name="manualPulse"></param>
        OnDaemonEventPulseDelegate OnPulse { get; set; }

        /// <summary>
        /// Returns a TimeSpan to inicate when the next OnEnqueueMessagesTimer Event should fire.
        /// </summary>
        /// <value></value>
        OnDaemonEventDetermineEnqueueMessagesTimerPeriodDelegate OnDetermineEnqueueMessagesTimerPeriod { get; set; }

        /// <summary>
        /// Event when a timer fires to allow for messages to get posted.  The event returns
        /// return a TimeSpan when the next event should fire.  If null is returned, then the
        /// next event will fire with the next auto pulse.  
        /// </summary>
        /// <value></value>
        OnDaemonEventDelegate OnEnqueueMessagesTimer { get; set; }

        /// <summary>
        /// Occurs at the start of dispatching messages.  This is a good place to provision resources
        /// that are needed for a block of work (as series of messages).  If the daemon should just process
        /// once per wake cycle, then drop one message in the queue and put the processing work in 
        /// `RunDispatchMessage`
        /// </summary>
        OnDaemonEventDelegate OnStartDispatch { get; set; }

        /// <summary>
        /// Occurs at the end of dispatching messages.  This is a good place to free resources
        /// that are needed for a block of work (as series of messages)
        /// </summary>
        OnDaemonEventDelegate OnEndDispatch { get; set; }

        /// <summary>
        /// General process teardown should go here
        /// </summary>
        OnDaemonEventDelegate OnTeardown { get; set; }
    }

    public interface IDaemonEngine<TMessage> : IDaemonEngine where TMessage : class {

        /// <summary>
        /// Posts a message for the daemon message thread.  Note that a daemon may add messages to it's own
        /// queue as well, perhaps checking for some condition at each pulse.  
        /// </summary>
        /// <param name="message">message object to post</param>
        /// <param name="triggerPulse">true if a pulse (wake event) should occur to process the message</param>
        void PostMessage(TMessage message, bool triggerPulse = true);

        /// <summary>
        /// Processes a message direclty and synchronously.  Not normally used.  This allows for 
        /// control in debugging scenarios.
        /// </summary>
        /// <returns>message id of processing message</returns>
        //Guid ProcessMessageDirectly(TMessage message);

        /// <summary>
        /// Put the processing main work of handling messages here. 
        /// </summary>
        /// <param name="message"></param>
        OnDaemonEventDispatchMessageDelegate<TMessage> OnDispatchMessage { get; set; }
    }

    public enum DaemonRunStateEnum {

        /// <summary>
        /// Daemon is not running
        /// </summary>
        Stopped,

        /// <summary>
        /// Daemon is starting up
        /// </summary>
        Starting,

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
        /// Daemon is resuming from a pause.  
        /// </summary>
        Resuming,

        /// <summary>
        /// Daemon is not running. Existing work is finishing
        /// before reaching a stopped state.
        /// </summary>
        Stopping
    }
}
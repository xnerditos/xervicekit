using System;
using XKit.Lib.Common.Config;
using XKit.Lib.Common.Log;

namespace XKit.Lib.Common.Host {

    /// <summary>
    /// Interface to a service daemon process.   
    /// </summary>
    /// <remarks>
    /// Service daemons are like the base daemon except that they are associated with a service.
    /// </remarks>
    public interface IServiceDaemon { 

        /// <summary>
        /// Name of the daemon
        /// </summary>
        /// <value></value>
        string Name { get; }

        /// <summary>
        /// Run state of the daemon process
        /// </summary>
        DaemonRunStateEnum RunState { get; }

        /// <summary>
        /// True if messages will be dispatched during a pulse 
        /// </summary>
        bool IsAutomaticMessageDispatchActive { get; }

        /// <summary>
        /// Starts the daemon process running
        /// </summary>
        void Start();

        /// <summary>
        /// Conveys that the config or process parameters have changed.
        /// </summary>
        /// <returns></returns>
        void SignalEnvironmentChange();

        /// <summary>
        /// Pulses the message thread wake up
        /// </summary>
        void Pulse();

        /// <summary>
        /// Pauses the daemon.  Existing tasks (messages in process) will finish, but no new tasks or threads
        /// should be spawned while paused.
        /// </summary>
        /// <returns></returns>
        void Pause();

        /// <summary>
        /// Resumes the daemon from a paused state.  
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
        /// Used to set "debug mode", which will stop automatc message dispatch and will request
        /// syncronous message processing.  
        /// </summary>
        /// <param name="debugModeOn"></param>
        void SetDebugMode(bool debugModeOn = true);

        /// <summary>
        /// Stops the daemon
        /// </summary>
        /// <returns></returns>
        void Stop();

        void AddToService(
            IServiceBase service
        );

        void PostMessage(object message, bool triggerPulse = true);

        //void ProcessMessageDirectly(object message);

        /// <summary>
        /// Forces the dispatch of a message immedately on the current thread.  Returns false if 
        /// no message available to dispatch
        /// </summary>
        bool DispatchMessagesDirectly(int count = 1);

        /// <summary>
        /// Gets the number of messages currently being processed
        /// </summary>
        /// <returns></returns>
        int GetActiveMessageCount();

        /// <summary>
        /// Gets the number of messages waiting to be processed
        /// </summary>
        /// <returns></returns>
        int GetWaitingMessageCount();

        /// <summary>
        /// Gets the total number of messages both processing and waiting
        /// </summary>
        /// <returns></returns>
        int GetTotalMessageCount();
    }

    public interface IServiceDaemon<TMessage> : IServiceDaemon 
        where TMessage : class {

        void PostMessage(TMessage message, bool triggerPulse = true);
        //void ProcessMessageDirectly(TMessage message);
    }

    public interface IServiceDaemonOperationOwner : IServiceDaemon { 

        /// <summary>
        /// Called by the operation when it is finished
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sender">the operation object</param>
        void SignalOperationFinished(Guid messageProcessingId);

        bool IsDebugMode { get; }
    }
}

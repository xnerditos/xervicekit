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
        /// Starts the daemon process running
        /// </summary>
        void Start();

        /// <summary>
        /// Conveys that the config or process parameters have changed.
        /// </summary>
        /// <returns></returns>
        void SignalEnvironmentChange();

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
        /// Used to set "debug mode", which will stop automatic message dispatch and will request
        /// synchronous message processing.  
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

        /// <summary>
        /// Causes the daemon to process messages.  If background is false, then message processing
        /// will happen synchronously on the current thread. 
        /// </summary>
        /// <param name="background"></param>
        void ProcessMessages(bool background = true);

        /// <summary>
        /// Processes one message synchronously for debugging scenarios.  Returns true of a message was processed. 
        /// </summary>
        /// <returns></returns>
        bool DebugProcessOneMessage();

        /// <summary>
        /// Processes one timer event synchronously for debugging scenarios.  Returns true of a message was processed. 
        /// </summary>
        /// <returns></returns>
        void DebugFireTimerEvent();

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

        /// <summary>
        /// Posts a single message to the daemon, optionally triggering background processing.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="triggerProcessing"></param>
        void PostMessage(TMessage message, bool triggerProcessing = true);

        /// <summary>
        /// Posts a single message to the daemon, optionally triggering background processing.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="triggerProcessing"></param>
        void PostMessages(TMessage[] message, bool triggerProcessing = true);

    }
}

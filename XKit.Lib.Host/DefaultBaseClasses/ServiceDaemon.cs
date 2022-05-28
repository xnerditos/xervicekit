using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Utility;
using XKit.Lib.Common.Utility.Collections;
using XKit.Lib.Common.Utility.Extensions;
using XKit.Lib.Common.Utility.Invocation;
using XKit.Lib.Common.Utility.Threading;
using XKit.Lib.Host.Helpers;

namespace XKit.Lib.Host.DefaultBaseClasses {

    public abstract class ServiceDaemon<TDaemonOperation, TMessage>
        : IServiceDaemon<TMessage>, IServiceDaemon, IServiceDaemonOperationOwner
        where TMessage : class 
        where TDaemonOperation : IServiceDaemonOperation<TMessage> {

        public enum BaseMonitorCodes {
            NullOperationFromMessage,
            ProcessingMessageStarted,
            MessageProcessResultedInError
        }

        private readonly SetOnceOrThrow<ILogSessionFactory> logSessionFactory = new();
        protected ILogSessionFactory LogSessionFactory => logSessionFactory.Value;
        private readonly SetOnceOrThrow<ILogSession> log = new();
        protected ILogSession Log => log.Value;
        private readonly SetOnceOrThrow<Guid> messageThreadOperationId = new();
        protected Guid MessageThreadOperationId => messageThreadOperationId.Value;
        private readonly SynchronizedList<IServiceDaemonOperation<TMessage>> processingOperations = new();
        private readonly IDaemonEngine<TMessage> engine;
        private Func<object[], object> operationInstantiator;

        public ServiceDaemon(
            ILogSessionFactory logSessionFactory, 
            IDaemonEngine<TMessage> engine = null
        ) { 
            this.engine = engine ?? new DaemonEngine<TMessage>();
            this.logSessionFactory.Value = logSessionFactory;
            engine.OnDispatchMessage = this.OnDispatchMessage;
            engine.OnStartup = this.OnStartup;
            engine.OnTeardown = this.OnTeardown;
            engine.OnPulse = this.OnPulse;
            engine.OnDetermineEnqueueMessagesTimerPeriod = this.OnDetermineEnqueueEventPeriod;
            engine.OnEnqueueMessagesTimer = this.OnEnqueueEvent;
        }

        // =====================================================================
        // IServiceDaemon
        // =====================================================================

        void IServiceDaemon.AddToService(
            IServiceBase service
        ) {
            this.Service = service;
        }

        void IServiceDaemon.Start() => engine.Start();

        void IServiceDaemon.SignalEnvironmentChange() {
            OnEnvironmentChange();
        }

        void IServiceDaemon.Pulse() => engine.Pulse();

        void IServiceDaemon.Pause() => engine.Pause();

        void IServiceDaemon.Resume() => engine.Resume();

        void IServiceDaemon.SuspendAutomaticMessageDispatch() => engine.SuspendAutomaticMessageDispatch();

        void IServiceDaemon.ResumeAutomaticMessageDispatch() => engine.ResumeAutomaticMessageDispatch();

        void IServiceDaemon.Stop() => engine.Stop();

        void IServiceDaemon.PostMessage(object message, bool triggerPulse) => PostMessage((TMessage)message, triggerPulse);

        void IServiceDaemon<TMessage>.PostMessage(TMessage message, bool triggerPulse) => PostMessage(message, triggerPulse);

        bool IServiceDaemon.DispatchMessagesDirectly(int count) {
            
            switch(engine.RunState) {
            case DaemonRunStateEnum.Resuming:
            case DaemonRunStateEnum.Starting:
                while(engine.RunState != DaemonRunStateEnum.Running) {
                    System.Threading.Thread.Sleep(200);
                }
                break;
            case DaemonRunStateEnum.Paused:
            case DaemonRunStateEnum.Stopped:
                return false;
            }

            bool processed = false;
            if (count > 0) {
                do {
                    processed = engine.DispatchMessageDirectly();
                    count--;
                } while(count > 0 && processed);
            }
            return processed;
        }

        void IServiceDaemon.SetDebugMode(bool debugModeOn) {
            engine.DebugMode = debugModeOn;
        }

        bool IServiceDaemon.IsAutomaticMessageDispatchActive => engine.IsAutomaticMessageDispatchActive;

        DaemonRunStateEnum IServiceDaemon.RunState => engine.RunState;

        string IServiceDaemon.Name => this.Name;        
        int IServiceDaemon.GetActiveMessageCount() => this.ActiveMessageCount;
        int IServiceDaemon.GetWaitingMessageCount() => this.WaitingMessageCount;
        int IServiceDaemon.GetTotalMessageCount() => this.ActiveMessageCount + this.WaitingMessageCount;
        
        // =====================================================================
        // IServiceDaemonOperationOwner
        // =====================================================================

        void IServiceDaemonOperationOwner.SignalOperationFinished(Guid messageProcessingId) {
            engine.SignalFinishMessageProcessing(messageProcessingId);
        }
        
        bool IServiceDaemonOperationOwner.IsDebugMode => engine.DebugMode;

        // =====================================================================
        // Daemon engine events
        // =====================================================================

        private void OnPulse(bool manualPulse) {
            PruneProcessingOperationsList();
        }

        private void OnDispatchMessage(Guid messageProcessingId, TMessage message) {

            var operation = CreateDaemonOperation(new ServiceDaemonOperationContext(
                this,
                this.Service,
                this.LocalEnvironment,
                messageProcessingId
            ));

            object messageSummary = GetMessageSummary(message);

            if (operation != null) {
                TaskUtil.RunSyncSafely(() => DispatchMessage(
                    operation,
                    message,
                    messageProcessingId,
                    messageSummary
                ));
            } else {
                Log.Warning(
                    "Operation created in response to a message was NULL",
                    attributes : new {
                        Identifier = messageProcessingId,
                            Message = messageSummary
                    },
                    code : BaseMonitorCodes.NullOperationFromMessage
                );
            }
        }
        
        /// <summary>
        /// Generates an object that represents a summary of the message for the log
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected virtual object GetMessageSummary(TMessage message) => message; // default is just to use the message itself

        private void OnStartup() {
            this.messageThreadOperationId.Value = Guid.NewGuid();
            this.log.Value = LogSessionFactory.CreateLogSession(
                originatorName: $"{Service.Descriptor.Collection}.{Service.Descriptor.Name}",
                originatorVersion: Service.Descriptor.Version,
                originatorFabricId: LocalEnvironment.FabricId,
                originatorInstanceId: Service.InstanceId
            );

            Log.Begin(
                LogContextTypeEnum.ServiceDaemonMain,
                this.Name + ".Main",
                contextId: MessageThreadOperationId
            );
            OnDaemonStarting();
        }

        private void OnTeardown() {
            OnDaemonStopping();
            Log.End(
                LogResultStatusEnum.Success
            );
        }

        protected virtual TimeSpan? OnDetermineEnqueueEventPeriod() => null;

        protected virtual void OnEnqueueEvent() { }
        protected virtual void OnDaemonStarting() { }
        protected virtual void OnDaemonStopping() { }

        // =====================================================================
        // abstract and virtual 
        // =====================================================================

        protected abstract string Name { get; }
        protected virtual void OnEnvironmentChange() { }

        protected virtual IServiceDaemonOperation<TMessage> CreateDaemonOperation(ServiceDaemonOperationContext context) {
            if (operationInstantiator == null) {
                var constructor = typeof(TDaemonOperation).GetConstructor(new[] { 
                    typeof(ServiceDaemonOperationContext)
                });
                if (constructor == null) {
                    throw new Exception("Cannot instantiate daemon operation");
                }
                operationInstantiator = MethodInvokerFactory.ForConstructor(constructor);
                if (operationInstantiator == null) {
                    throw new Exception("Operation Instantiator could not be created");
                }
            }
            return (IServiceDaemonOperation<TMessage>) operationInstantiator(new[] { context });
        }

        // =====================================================================
        // private
        // =====================================================================

        private async Task DispatchMessage(
            IServiceDaemonOperation<TMessage> operation,
            TMessage message,
            Guid messageProcessingId,
            object messageSummary
        ) {
            Log.Trace(
                "Processing message",
                attributes : new {
                    Identifier = messageProcessingId,
                        Message = messageSummary
                },
                code : BaseMonitorCodes.ProcessingMessageStarted
            );

            var result = await operation.RunDaemonOperation(message);
            if (result.HasError) {
                engine.SignalFinishMessageProcessing(messageProcessingId);
                Log.Warning(
                    "Message resulted in error",
                    attributes : new { Identifier = messageProcessingId },
                    code : BaseMonitorCodes.MessageProcessResultedInError
                );
            } else if (result.IsPending) {
                Log.Status(
                    "Message transitioning to background"
                );
                processingOperations.Add(operation);
            } else {
                Log.Status(
                    "Message finished in the foreground"
                );
            }
        }

        private void PruneProcessingOperationsList() {
            foreach (var op in processingOperations.ToArray()) {
                if (!op.IsActive) {
                    processingOperations.Remove(op);
                }
            }
        }

        // =====================================================================
        // Other protected
        // =====================================================================

        protected IHostEnvironment HostEnvironment => Service.HostEnvironment;
        protected ILocalEnvironment LocalEnvironment => Service.LocalEnvironment;
        protected IServiceBase Service { get; private set; }

        //protected IDaemonEngine Engine => this.engine;

        protected int WakeDelayMillisecondsWhenNoMessagesWaiting { 
            get => engine.WakeDelayMillisecondsNoWork; 
            set => engine.WakeDelayMillisecondsNoWork = value; 
        }

        protected int TimeoutToStopMilliseconds { 
            get => engine.TimeoutToStopMilliseconds;
            set => engine.TimeoutToStopMilliseconds = value; 
        }

        protected int WakeDelayMillisecondsWhenMessagesWaiting {
            get => engine.WakeDelayMillisecondsWorkWaiting;
            set => engine.WakeDelayMillisecondsWorkWaiting = value;
        }

        protected int MaxConcurrentMessages {
            get => engine.MaxConcurrentMessages;
            set => engine.MaxConcurrentMessages = value;
        }

        protected bool AutoPulseActive { 
            get => engine.AutoPulseActive;
            set => engine.AutoPulseActive = value;
        }

        protected bool EnableEnqueueEvent { 
            get => engine.EnableEnqueueEvent; 
            set => engine.EnableEnqueueEvent = value; 
        }

        protected int ActiveMessageCount => engine.ActiveMessageCount;
        protected int WaitingMessageCount => engine.WaitingMessageCount;
        protected bool HasMessages => engine.HasMessages;

        protected void Pulse() => engine.Pulse();

        protected void PostMessage(TMessage message, bool triggerPulse = true) => engine.PostMessage(message, triggerPulse);

        //protected void ProcessMessageDirectly(TMessage message) => engine.ProcessMessageDirectly(message);

        // =====================================================================
        // Monitoring and Log
        // =====================================================================

        /// <summary>
        /// Log an erratum (bug, unexpected behaviour)
        /// </summary>
        protected void Erratum(
            string message = null,
            IReadOnlyDictionary<string, object> attributes = null,
            IEnumerable<string> tags = null,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = 0
        ) => Log.ErratumAs(
                message,
                attributes,
                tags,
                callerFilePath,
                callerMemberName,
                callerLineNumber
            );

        /// <summary>
        /// Log an erratum (bug, unexpected behaviour)
        /// </summary>
        protected void Erratum(
            string message,
            object attributes,
            IEnumerable<string> tags = null,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = 0
        ) => Log.ErratumAs(
                message,
                attributes?.FieldsToDictionary(),
                tags,
                callerFilePath,
                callerMemberName,
                callerLineNumber
            );

        /// <summary>
        /// Log an error (a plausible error, as opposed to unexpected behaviour). 
        /// </summary>
        protected void Error(
            string message = null,
            IReadOnlyDictionary<string, object> attributes = null,
            object code = null,
            IEnumerable<string> tags = null,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = 0
        ) => Log.ErrorAs(
                message,
                attributes,
                code,
                tags,
                callerFilePath,
                callerMemberName,
                callerLineNumber
            );

        /// <summary>
        /// Log an error (a plausible error, as opposed to unexpected behaviour)
        /// </summary>
        protected void Error(
            string message,
            object attributes,
            object code = null,
            IEnumerable<string> tags = null,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = 0
        ) => Log.ErrorAs(
                message,
                attributes?.FieldsToDictionary(),
                code,
                tags,
                callerFilePath,
                callerMemberName,
                callerLineNumber
            );

        /// <summary>
        /// Log a warning 
        /// </summary>
        protected void Warning(
            string message = null,
            IReadOnlyDictionary<string, object> attributes = null,
            object code = null,
            IEnumerable<string> tags = null,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = 0
        ) => Log.WarningAs(
            message,
            attributes,
            code,
            tags,
            callerFilePath,
            callerMemberName,
            callerLineNumber
        );

        /// <summary>
        /// Log a warning 
        /// </summary>
        protected void Warning(
            string message,
            object attributes,
            object code = null,
            IEnumerable<string> tags = null,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = 0
        ) => Log.WarningAs(
            message,
            attributes?.FieldsToDictionary(),
            code,
            tags,
            callerFilePath,
            callerMemberName,
            callerLineNumber
        );

        /// <summary>
        /// Log a status update 
        /// </summary>
        protected void Status(
            string message,
            IReadOnlyDictionary<string, object> attributes = null,
            object code = null,
            IEnumerable<string> tags = null
        ) => Log.Status(
            message,
            attributes,
            code,
            tags
        );
        
        /// <summary>
        /// Log a status update 
        /// </summary>
        protected void Status(
            string message,
            object attributes,
            object code = null,
            IEnumerable<string> tags = null
        ) => Log.Status(
            message,
            attributes?.FieldsToDictionary(),
            code,
            tags
        );

        /// <summary>
        /// Log a trace message (for debugging and analysis) 
        /// </summary>
        protected void Trace(
            string message,
            IReadOnlyDictionary<string, object> attributes,
            object code = null,
            IEnumerable<string> tags = null,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = 0
        ) => Log.TraceAs(
            message,
            attributes,
            code,
            tags,
            callerFilePath,
            callerMemberName,
            callerLineNumber
        );
                
        /// <summary>
        /// Log a trace message (for debugging and analysis) 
        /// </summary>
        protected void Trace(
            string message,
            object attributes,
            object code,
            IEnumerable<string> tags = null,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = 0
        ) => Log.TraceAs(
            message,
            attributes?.FieldsToDictionary(),
            code,
            tags,
            callerFilePath,
            callerMemberName,
            callerLineNumber
        );

        /// <summary>
        /// Log a trace message (for debugging and analysis) 
        /// </summary>
        protected void Trace(
            string message,
            object attributes,
            IEnumerable<string> tags = null,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = 0
        ) => Log.TraceAs(
            message,
            attributes?.FieldsToDictionary(),
            null,
            tags,
            callerFilePath,
            callerMemberName,
            callerLineNumber
        );

        /// <summary>
        /// Log a trace message (for debugging and analysis) 
        /// </summary>
        protected void Trace(
            object attributes,
            IEnumerable<string> tags = null,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = 0
        ) => Log.TraceAs(
            "",
            attributes?.FieldsToDictionary(),
            null,
            tags,
            callerFilePath,
            callerMemberName,
            callerLineNumber
        );

        /// <summary>
        /// Log a trace message (for debugging and analysis) 
        /// </summary>
        protected void Trace(
            string message,
            IEnumerable<string> tags = null,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = 0
        ) => Log.TraceAs(
            message,
            (Dictionary<string, object>)null,
            null,
            tags,
            callerFilePath,
            callerMemberName,
            callerLineNumber
        );

        /// <summary>
        /// Log a trace message (for debugging and analysis) 
        /// </summary>
        protected void Trace(
            IEnumerable<string> tags = null,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = 0
        ) => Log.TraceAs(
            "",
            (Dictionary<string, object>)null,
            null,
            tags,
            callerFilePath,
            callerMemberName,
            callerLineNumber
        );

        /// <summary>
        /// Log information that is part of an audit trail
        /// </summary>
        protected void Audit(
            string message, 
            object attributes = null, 
            object code = null, 
            IEnumerable<string> tags = null
        ) => Log.Audit(
            message,
            attributes,
            code,
            tags            
        );

        /// <summary>
        /// Log information that is part of an audit trail
        /// </summary>
        protected void Audit(
            string message, 
            IReadOnlyDictionary<string, object> attributes = null, 
            object code = null, 
            IEnumerable<string> tags = null
        ) => Log.Audit(
            message,
            attributes,
            code,
            tags            
        );

        /// <summary>
        /// Log info (generally for debugging and analysis.  If otherwise, consider Status instead) 
        /// </summary>
        protected void Info(
            string message = null,
            IReadOnlyDictionary<string, object> attributes = null,
            object data = null,
            object code = null, 
            IEnumerable<string> tags = null
        ) => Log.Info(
            message,
            attributes,
            data,
            code,
            tags
        );
        
        /// <summary>
        /// Log info (generally for debugging and analysis.  If otherwise, consider Status instead) 
        /// </summary>
        protected void Info(
            string message,
            object attributes,
            object data = null,
            object code = null, 
            IEnumerable<string> tags = null
        ) => Log.Info(
            message,
            attributes?.FieldsToDictionary(),
            data,
            code,
            tags
        );

        /// <summary>
        /// Log a snapshot of operation data.  This may be used by downstream processes analyzing the
        /// operation for specific purposes. 
        /// </summary>
        protected void Snapshot(
            IReadOnlyDictionary<string, object> attributes,
            object data = null,
            object code = null,
            IEnumerable<string> tags = null
        ) => Log.Snapshot(
            attributes,
            data,
            code,
            tags
        );

        /// <summary>
        /// Log a snapshot of operation data.  This may be used by downstream processes analyzing the
        /// operation for specific purposes. 
        /// </summary>
        protected void Snapshot(
            object attributes,
            object data = null,
            object code = null,
            IEnumerable<string> tags = null
        ) => Log.Snapshot(
            attributes?.FieldsToDictionary(),
            data,
            code,
            tags
        );

        /// <summary>
        /// Automatically log a value on exit.
        /// </summary>
        protected void AutoLog(
            string name,
            object value
        ) => Log.AutoLog(name, value);

        /// <summary>
        /// Automatically log a value on exit.
        /// </summary>
        protected void AutoLog(
            object values
        ) => Log.AutoLog(values);
    }

    public abstract class ServiceDaemon<TDaemonOperation> : ServiceDaemon<TDaemonOperation, object> 
        where TDaemonOperation : IServiceDaemonOperation<object> { 

        public ServiceDaemon(
            ILogSessionFactory logSessionFactory, 
            IDaemonEngine<object> engine = null
        ) : base(logSessionFactory, engine) { }
    }
}

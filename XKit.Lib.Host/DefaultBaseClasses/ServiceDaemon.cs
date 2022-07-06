using System;
using System.Threading.Tasks;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Utility;
using XKit.Lib.Common.Utility.Invocation;
using XKit.Lib.Common.Utility.Threading;
using XKit.Lib.Host.Helpers;

namespace XKit.Lib.Host.DefaultBaseClasses {

    public abstract class ServiceDaemon<TDaemonOperation, TMessage>
        : IServiceDaemon<TMessage>, IServiceDaemon
        where TMessage : class 
        where TDaemonOperation : IServiceDaemonOperation<TMessage> {

        public enum BaseLogCodes {
            ProcessingMessageStarted,
            MessageProcessResultedInError
        }

        private readonly SetOnceOrThrow<ILogSessionFactory> logSessionFactory = new();
        protected ILogSessionFactory LogSessionFactory => logSessionFactory.Value;
        private readonly SetOnceOrThrow<ILogSession> log = new();
        protected ILogSession Log => log.Value;
        private readonly SetOnceOrThrow<Guid> messageThreadOperationId = new();
        protected Guid MessageThreadOperationId => messageThreadOperationId.Value;
        private readonly IDaemonEngine<TMessage> engine;
        private Func<object[], object> operationInstantiator;

        public ServiceDaemon(
            ILogSessionFactory logSessionFactory, 
            IDaemonEngine<TMessage> engine = null
        ) { 
            this.engine = engine ?? new DaemonEngine<TMessage>();
            this.logSessionFactory.Value = logSessionFactory;
            this.engine.OnProcessMessage = this.OnProcessMessage;
            this.engine.OnDetermineTimerPeriod = this.OnDetermineEnqueueEventPeriod;
            this.engine.OnTimerEvent = this.OnTimerEvent;
            this.engine.OnStartProcessMessageBatch = OnBeginProcessingMessages;
            this.engine.OnEndProcessMessageBatch = OnEndProcessingMessages;
        }

        // =====================================================================
        // IServiceDaemon
        // =====================================================================

        void IServiceDaemon.AddToService(
            IServiceBase service
        ) {
            this.Service = service;
        }

        void IServiceDaemon.Start() {
            messageThreadOperationId.Value = Guid.NewGuid();
            log.Value = LogSessionFactory.CreateLogSession(
                originatorName: $"{Service.Descriptor.Collection}.{Service.Descriptor.Name}",
                originatorVersion: Service.Descriptor.Version,
                originatorFabricId: HostEnvironment.FabricId,
                originatorInstanceId: Service.InstanceId
            );

            Log.Begin(
                LogContextTypeEnum.ServiceDaemonMain,
                this.Name + ".Main",
                contextId: MessageThreadOperationId
            );
            OnDaemonStarting();
            engine.Start();
        }

        void IServiceDaemon.SignalEnvironmentChange() {
            OnEnvironmentChange();
        }

        void IServiceDaemon.Pause() => engine.Pause();

        void IServiceDaemon.Resume() => engine.Resume();

        void IServiceDaemon.Stop() {
            engine.Stop();
            OnDaemonStopping();
            Log.End(
                LogResultStatusEnum.Success
            );
        }

        void IServiceDaemon<TMessage>.PostMessage(TMessage message, bool triggerProcessing) => PostMessage(message, triggerProcessing);

        void IServiceDaemon<TMessage>.PostMessages(TMessage[] messages, bool triggerProcessing) => PostMessages(messages, triggerProcessing);

        bool IServiceDaemon.DebugProcessOneMessage() => engine.ProcessOneMessageSync();
        void IServiceDaemon.SetDebugMode(bool debugModeOn) {
            engine.DebugMode = debugModeOn;
        }

        DaemonRunStateEnum IServiceDaemon.RunState => engine.RunState;

        string IServiceDaemon.Name => this.Name;        
        int IServiceDaemon.GetActiveMessageCount() => this.ActiveMessageCount;
        int IServiceDaemon.GetWaitingMessageCount() => this.WaitingMessageCount;
        int IServiceDaemon.GetTotalMessageCount() => this.ActiveMessageCount + this.WaitingMessageCount;
        void IServiceDaemon.ProcessMessages(bool background) => ProcessMessages(background);
        
        // =====================================================================
        // Daemon engine events
        // =====================================================================

        private void OnProcessMessage(Guid messageProcessingId, TMessage message) {

            var operation = CreateDaemonOperation(new ServiceDaemonOperationContext(
                this,
                this.Service,
                this.HostEnvironment,
                messageProcessingId
            ));

            object messageSummary = GetMessageSummary(message);

            if (operation != null) {
                TaskUtil.RunAsyncAsSync(() => DispatchMessageViaOperation(
                    operation,
                    message,
                    messageProcessingId,
                    messageSummary
                ));
            } else {
                Log.Erratum(
                    "Operation created in response to a message was NULL",
                    attributes : new {
                        Identifier = messageProcessingId,
                            Message = messageSummary
                    }
                );
            }
        }
        
        /// <summary>
        /// Generates an object that represents a summary of the message for the log
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected virtual object GetMessageSummary(TMessage message) => message; // default is just to use the message itself

        protected virtual uint? OnDetermineEnqueueEventPeriod() => null;

        protected virtual void OnTimerEvent() { }
        protected virtual void OnDaemonStarting() { }
        protected virtual void OnDaemonStopping() { }
        protected virtual void OnBeginProcessingMessages() { }
        protected virtual void OnEndProcessingMessages() { }

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

        private async Task DispatchMessageViaOperation(
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
                code : BaseLogCodes.ProcessingMessageStarted
            );

            var result = await operation.RunDaemonOperation(message);
            if (result.HasError) {
                Log.Error(
                    result.Message,
                    attributes : new { Identifier = messageProcessingId },
                    code : BaseLogCodes.MessageProcessResultedInError
                );
            } else if (result.IsPending) {
                Log.Erratum(
                    "Daemon operation result was pending.  Daemon operations should not return as pending but should complete synchronously."
                );
            } 
        }

        // =====================================================================
        // Other protected
        // =====================================================================

        protected IXKitHostEnvironment HostEnvironment => Service.HostEnvironment;
        protected IServiceBase Service { get; private set; }

        //protected IDaemonEngine Engine => this.engine;

        protected int MaxConcurrentMessages {
            get => engine.MaxConcurrentMessages;
            set => engine.MaxConcurrentMessages = value;
        }

        protected uint DefaultTimerPeriodMilliseconds { 
            get => engine.DefaultTimerPeriodMilliseconds;
            set => engine.DefaultTimerPeriodMilliseconds = value;
        }

        protected bool EnableTimerEvent { 
            get => engine.EnableTimer; 
            set => engine.EnableTimer = value; 
        }

        protected int ActiveMessageCount => engine.ActiveMessageCount;
        protected int WaitingMessageCount => engine.WaitingMessageCount;
        protected bool HasMessages => engine.HasMessages;

        protected void PostMessage(TMessage message, bool triggerProcessing = true) => engine.PostMessage(message, triggerProcessing);
        protected void PostMessages(TMessage[] messages, bool triggerProcessing = true) => engine.PostMessages(messages, triggerProcessing);
        protected void ProcessMessages(bool background = true) => engine.ProcessMessages(background);
    }

    public abstract class ServiceDaemon<TDaemonOperation> : ServiceDaemon<TDaemonOperation, object> 
        where TDaemonOperation : IServiceDaemonOperation<object> { 

        public ServiceDaemon(
            ILogSessionFactory logSessionFactory, 
            IDaemonEngine<object> engine = null
        ) : base(logSessionFactory, engine) { }
    }
}

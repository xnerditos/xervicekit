using System;
using System.Threading.Tasks;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Utility;
using XKit.Lib.Common.Utility.Invocation;
using XKit.Lib.Common.Utility.Threading;
using XKit.Lib.Host.Helpers;

namespace XKit.Lib.Host.DefaultBaseClasses {

    public abstract class ServiceDaemon<TDaemonMessageOperation, TMessage, TDaemonTimerOperation>
        : IServiceDaemon<TMessage>, IServiceDaemon
        where TMessage : class 
        where TDaemonMessageOperation : IServiceDaemonOperation<TMessage> 
        where TDaemonTimerOperation : IServiceDaemonOperation {

        public enum BaseLogCodes {
            ProcessingMessageStarted,
            ProcessingTimerStarted,
            MessageProcessResultedInError,
            TimerProcessResultedInError
        }

        private readonly SetOnceOrThrow<ILogSessionFactory> logSessionFactory = new();
        protected ILogSessionFactory LogSessionFactory => logSessionFactory.Value;
        private readonly SetOnceOrThrow<ILogSession> log = new();
        protected ILogSession Log => log.Value;
        private readonly SetOnceOrThrow<Guid> messageThreadOperationId = new();
        protected Guid MessageThreadOperationId => messageThreadOperationId.Value;
        private readonly IDaemonEngine<TMessage> engine;
        private Func<object[], object> messageOperationInstantiator;
        private Func<object[], object> timerOperationInstantiator;

        public ServiceDaemon(
            ILogSessionFactory logSessionFactory, 
            IDaemonEngine<TMessage> engine = null
        ) { 
            this.engine = engine ?? new DaemonEngine<TMessage>();
            this.logSessionFactory.Value = logSessionFactory;
            this.engine.OnProcessMessage = this.OnProcessMessage;
            this.engine.OnDetermineTimerPeriod = this.OnDetermineTimerEventPeriod;
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

        protected virtual void OnProcessMessage(Guid messageProcessingId, TMessage message) {

            var operation = CreateMessageOperation(new ServiceDaemonOperationContext(
                this,
                this.Service,
                this.HostEnvironment
            ));

            if (operation != null) {
                TaskUtil.RunAsyncAsSync(() => DispatchMessageViaOperation(
                    operation,
                    message,
                    messageProcessingId
                ));
            } else {
                Log.Erratum(
                    "Operation created in response to a message was NULL",
                    attributes : new {
                        Identifier = messageProcessingId
                    }
                );
            }
        }
        
        protected virtual uint? OnDetermineTimerEventPeriod() => null;

        protected virtual void OnTimerEvent() { 

            var operation = CreateTimerOperation(new ServiceDaemonOperationContext(
                this,
                this.Service,
                this.HostEnvironment
            ));

            if (operation != null) {
                TaskUtil.RunAsyncAsSync(() => DispatchTimerEventViaOperation(
                    operation
                ));
            } else {
                Log.Erratum(
                    "Operation created in response for timer was NULL"
                );
            }
        }

        protected virtual void OnDaemonStarting() { }
        protected virtual void OnDaemonStopping() { }
        protected virtual void OnBeginProcessingMessages() { }
        protected virtual void OnEndProcessingMessages() { }

        // =====================================================================
        // abstract and virtual 
        // =====================================================================

        protected abstract string Name { get; }
        protected virtual void OnEnvironmentChange() { }

        protected virtual IServiceDaemonOperation<TMessage> CreateMessageOperation(
            ServiceDaemonOperationContext context
        ) {
            if (messageOperationInstantiator == null) {
                var constructor = typeof(TDaemonMessageOperation).GetConstructor(new[] { 
                    typeof(ServiceDaemonOperationContext)
                });
                if (constructor == null) {
                    throw new Exception("Cannot instantiate daemon message operation");
                }
                messageOperationInstantiator = MethodInvokerFactory.ForConstructor(constructor);
                if (messageOperationInstantiator == null) {
                    throw new Exception("Operation Instantiator could not be created for message operation");
                }
            }
            return (IServiceDaemonOperation<TMessage>) messageOperationInstantiator(new object[] { context });
        }

        protected virtual IServiceDaemonOperation CreateTimerOperation(ServiceDaemonOperationContext context) {
            if (timerOperationInstantiator == null) {
                var constructor = typeof(TDaemonTimerOperation).GetConstructor(new[] { 
                    typeof(ServiceDaemonOperationContext)
                });
                if (constructor == null) {
                    throw new Exception("Cannot instantiate daemon timer operation");
                }
                timerOperationInstantiator = MethodInvokerFactory.ForConstructor(constructor);
                if (messageOperationInstantiator == null) {
                    throw new Exception("Operation Instantiator could not be created for timer operation");
                }
            }
            return (IServiceDaemonOperation) messageOperationInstantiator(new[] { context });
        }

        // =====================================================================
        // private
        // =====================================================================

        private async Task DispatchMessageViaOperation(
            IServiceDaemonOperation<TMessage> operation,
            TMessage message,
            Guid messageProcessingId
        ) {
            Log.Trace(
                "Processing message",
                attributes : new {
                    Identifier = messageProcessingId
                },
                code : BaseLogCodes.ProcessingMessageStarted
            );

            var result = await operation.RunDaemonMessageOperation(message);
            if (result.HasError) {
                Log.Error(
                    result.Message,
                    attributes : new { Identifier = messageProcessingId },
                    code : BaseLogCodes.MessageProcessResultedInError
                );
            } else if (result.IsPending) {
                Log.Erratum(
                    "Daemon message operation result was pending.  Daemon operations should not return as pending but should complete synchronously."
                );
            } 
        }

        private async Task DispatchTimerEventViaOperation(
            IServiceDaemonOperation operation
        ) {
            Log.Trace(
                "Processing timer event via operation",
                code : BaseLogCodes.ProcessingMessageStarted
            );

            var result = await operation.RunDaemonTimerOperation();
            if (result.HasError) {
                Log.Error(
                    result.Message,
                    code : BaseLogCodes.TimerProcessResultedInError
                );
            } else if (result.IsPending) {
                Log.Erratum(
                    "Daemon timer operation result was pending.  Daemon operations should not return as pending but should complete synchronously."
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

    // public abstract class ServiceDaemon<TDaemonTimerOperation> : ServiceDaemon<TDaemonTimerOperation, object> 
    //     where TDaemonTimerOperation : IServiceDaemonOperation<object> { 

    //     public ServiceDaemon(
    //         ILogSessionFactory logSessionFactory, 
    //         IDaemonEngine<object> engine = null
    //     ) : base(logSessionFactory, engine) { }
    // }
}

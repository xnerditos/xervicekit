using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using System.Threading.Tasks;
using XKit.Lib.Common.Log;

namespace XKit.Lib.Host.DefaultBaseClasses {

    public abstract class ServiceDaemonOperation : Operation, IServiceDaemonOperation {

        protected new ServiceDaemonOperationContext Context => base.Context as ServiceDaemonOperationContext;
        protected IServiceBase Service => Context.Service;
        protected IServiceDaemon Daemon => Context.Daemon;

        public ServiceDaemonOperation(
            ServiceDaemonOperationContext context
        ) : base(context) { }

        protected virtual bool IsOperationSynchronous => false;

        // =====================================================================
        // base class implementations
        // =====================================================================
        
        protected override string OriginatorName => $"{Service.Descriptor.Collection}.{Service.Descriptor.Name}.{Context.Daemon.Name}";

        protected override int OriginatorVersion => Service.Descriptor.Version;

        protected override string OriginatorInstanceId => Service.InstanceId;

        protected override bool CanStartNewOperation() => Service.CanStartNewOperation();

        protected override LogContextTypeEnum OperationType => LogContextTypeEnum.ServiceDaemonOperation;

        // =====================================================================
        // virtual and abstract
        // =====================================================================

        /// <summary>
        /// Override to do the timer operation logic.
        /// </summary>
        /// <param name="message"></param>
        protected virtual Task DoTimerOperation() => Task.CompletedTask; 

        protected virtual Task DoPreOperation() => Task.CompletedTask;
        protected virtual Task DoPostOperation() => Task.CompletedTask;

        // =====================================================================
        // IServiceDaemonOperation implementation
        // =====================================================================

        async Task<OperationResult> IServiceDaemonOperation.RunDaemonTimerOperation() {
            var result = await RunOperation(
                operationName: OriginatorName,
                runSynchronous: Daemon.DebugMode,  // when not in debug mode, run async
                operationAction: DoTimerOperation,                
                preOperationAction: DoPreOperation,
                postOperationAction: (_) => DoPostOperation()
            );
            return result;
        }
    }

    public abstract partial class ServiceDaemonOperation<TMessage> 
        : ServiceDaemonOperation, IServiceDaemonOperation<TMessage> 
        where TMessage : class {
        
        protected ServiceDaemonOperation(
            ServiceDaemonOperationContext context
        ) : base(context) { }

        protected override LogContextTypeEnum OperationType => LogContextTypeEnum.ServiceDaemonOperation;

        // =====================================================================
        // virtual and abstract
        // =====================================================================

        /// <summary>
        /// Override to provide validation for the message.  Return true if the message
        /// is valid.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected virtual bool ValidateMessage(TMessage message) => true;

        /// <summary>
        /// Override to do the message processing operation
        /// </summary>
        /// <param name="message"></param>
        protected virtual Task<OperationResult> DoMessageOperation(TMessage message) => Task.FromResult(CreateResultSuccess());

        // =====================================================================
        // IServiceDaemonOperation<TMessage> implementation
        // =====================================================================

        async Task<OperationResult> IServiceDaemonOperation<TMessage>.RunDaemonMessageOperation(TMessage message) {
            var result = await RunOperation(
                operationName: OriginatorName,
                workItem: message,
                runSynchronous: true,
                operationAction: DoMessageOperation,                
                workItemValidationAction: ValidateMessage,
                preOperationAction: (_) => DoPreOperation(),
                postOperationAction: (_,_) => DoPostOperation()
            );
            return result;
        }
    }
}

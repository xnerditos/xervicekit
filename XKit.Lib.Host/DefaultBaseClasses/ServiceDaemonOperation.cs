using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using System.Threading.Tasks;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Utility.Threading;

namespace XKit.Lib.Host.DefaultBaseClasses {

    public abstract class ServiceDaemonOperation : Operation {

        new protected ServiceDaemonOperationContext Context => base.Context as ServiceDaemonOperationContext;
        protected IServiceBase Service => Context.Service;
        protected IServiceDaemonOperationOwner Daemon => Context.Daemon;

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
        /// Override to do the operation logic without providing an explicit result.  Override
        /// this method _or_ DoOperationLogicWithResult().
        /// </summary>
        /// <param name="message"></param>
        protected virtual Task DoOperationLogic(TMessage message) => Task.CompletedTask; 

        /// <summary>
        /// Override to do the operation logic with an explicit result.  Override
        /// this method _or_ DoOperationLogic().
        /// </summary>
        /// <param name="message"></param>
        protected async virtual Task<OperationResult> DoOperationLogicWithResult(TMessage message) {
            await DoOperationLogic(message);
            return ResultSuccess();
        }

        // =====================================================================
        // IServiceDaemonOperation<TMessage> implementation
        // =====================================================================

        async Task<OperationResult> IServiceDaemonOperation<TMessage>.RunDaemonOperation(TMessage message) {
            var result = await base.RunOperation<TMessage>(
                operationName: this.OriginatorName,
                workItem: message,
                runSynchronous: IsOperationSynchronous || Daemon.IsDebugMode,
                operationAction: DoOperationLogicWithResult,                
                workItemValidationAction: ValidateMessage,
                initAction: null,
                postOperationAction: (req, jr) => TeardownOperation()
            );
            return result;
        }

        private Task<bool> InitOperation() {
            return Task.FromResult(true);
        }

        private Task TeardownOperation() => TaskUtil.WrapInTask(() => {
            this.Daemon.SignalOperationFinished(Context.MessageProcessingId);
            return Task.CompletedTask;
        });
    }

    public abstract class ServiceDaemonOperation<TMessage, TServiceBase> 
        : ServiceDaemonOperation<TMessage>, IServiceDaemonOperation<TMessage> 
        where TMessage : class where TServiceBase : IServiceBase {

        new protected TServiceBase Service => (TServiceBase) base.Service;

        protected ServiceDaemonOperation(
            ServiceDaemonOperationContext context
        ) : base(context) { }
    }
}

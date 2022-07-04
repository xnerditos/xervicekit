using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Log;
using System.Threading.Tasks;
using System;

namespace XKit.Lib.Host.DefaultBaseClasses {

    public partial class ServiceOperation : Operation, IServiceOperation {

        protected new ServiceOperationContext Context => base.Context as ServiceOperationContext;
        protected ServiceCallTypeParameters CallTypeParameters => Context.CallParameters;
        protected IServiceCallRouter CallbackRouter { get; private set; }
        protected IServiceOperationOwner OperationOwner => Context.Service;
        protected IServiceBase Service => Context.Service;

        public ServiceOperation(
            ServiceOperationContext context
        ) : base(context) { }

        // =====================================================================
        // protected utility 
        // =====================================================================

        protected new ServiceCallResult ResultCallInvalidServiceUnavailable(
            string operationMessage = null
        ) => CreateServiceCallResult(
                Operation.ResultCallInvalidServiceUnavailable(operationMessage)
            );

        protected new ServiceCallResult<TResponseBody> ResultCallInvalidServiceUnavailable<TResponseBody>(
            string operationMessage = null
        ) where TResponseBody : class => CreateServiceCallResult<TResponseBody>(
            Operation.ResultCallInvalidServiceUnavailable<TResponseBody>(operationMessage)
        );

        protected new ServiceCallResult ResultAndLogCallPending(
            string operationMessage = null,
            string logMessage = null,
            object pendingLogParameters = null
        ) => CreateServiceCallResult(
            base.ResultAndLogCallPending(operationMessage, logMessage, pendingLogParameters)
        );

        protected new ServiceCallResult<TResponseBody> ResultAndLogCallPending<TResponseBody>(
            string operationMessage = null,
            string logMessage = null,
            object pendingLogParameters = null
        ) where TResponseBody : class => CreateServiceCallResult<TResponseBody>(
            base.ResultAndLogCallPending<TResponseBody>(operationMessage, logMessage, pendingLogParameters)
        );

        protected ServiceCallResult<TResponseBody> CreateServiceCallResult<TResponseBody>(
            OperationResult<TResponseBody> operationResult
        ) where TResponseBody : class => new() {
            ServiceCallStatus = ServiceCallStatusEnum.Completed,
            Service = this.Service.Descriptor.Clone(),
            Code = operationResult.Code,
            OperationId = this.Context.OperationId,
            Timestamp = DateTime.UtcNow,
            ResponderFabricId = Context.HostEnv.FabricId,
            ResponderInstanceId = Service.InstanceId,
            Message = operationResult.Message,
            OperationStatus = operationResult.OperationStatus,
            OperationName = this.OperationName,
            CorrelationId = Context.CorrelationId,
            RequestorFabricId = Context.RequestorFabricId,
            RequestorInstanceId = Context.RequestorInstanceId,
            ServiceStatus = Service.GetServiceStatus(),
            ResponseBody = operationResult.ResultData
        };

        protected ServiceCallResult CreateServiceCallResult(
            OperationResult operationResult
        ) => new() {
            ServiceCallStatus = ServiceCallStatusEnum.Completed,
            Service = this.Service.Descriptor.Clone(),
            Code = operationResult.Code,
            OperationId = Context.OperationId,
            Timestamp = DateTime.UtcNow,
            ResponderFabricId = Context.HostEnv.FabricId,
            ResponderInstanceId = Service.InstanceId,
            Message = operationResult.Message,
            OperationStatus = operationResult.OperationStatus,
            OperationName = this.OperationName,
            CorrelationId = Context.CorrelationId,
            RequestorFabricId = Context.RequestorFabricId,
            RequestorInstanceId = Context.RequestorInstanceId,
            ServiceStatus = Service.GetServiceStatus()
        };

        protected bool IsOperationSynchronous { 
            get {
                switch ((CallTypeParameters?.CallType).GetValueOrDefault(ServiceCallTypeEnum.SyncResult)) {
                case ServiceCallTypeEnum.FireAndForget:
                    return false;
                case ServiceCallTypeEnum.SyncResult:
                default:
                    return true;
                }
            }
        }

        // =====================================================================
        // base class implementations
        // =====================================================================
        
        protected override LogContextTypeEnum OperationType => LogContextTypeEnum.ServiceOperation;
        
        protected override string OriginatorName => $"{Service.Descriptor.Collection}.{Service.Descriptor.Name}";

        protected override int OriginatorVersion => Service.Descriptor.Version;

        protected override string OriginatorInstanceId => Service.InstanceId;

        protected override bool CanStartNewOperation() => Service.CanStartNewOperation();

        // =====================================================================
        // virtual and abstract
        // =====================================================================

        /// <summary>
        /// Override to do actions that are common as init pre-conditions to for all service calls 
        /// provided by this operation.  This method runs on the initial call thread, not 
        /// in the background if an operation moves into a pending result state.
        /// </summary>
        /// <returns>true if preconditions met</returns>
        protected virtual Task<bool> InitServiceOperation() => Task.FromResult(true);

        // =====================================================================
        // IServiceOperation
        // =====================================================================

    }

    public class ServiceOperation<TService> 
        : ServiceOperation where TService : class, IServiceBase {
        
        public ServiceOperation(
            ServiceOperationContext context
        ) : base(context) { }

        public new TService Service => (TService)base.Service; 
    }
}

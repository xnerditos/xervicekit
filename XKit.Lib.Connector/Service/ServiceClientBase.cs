using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using XKit.Lib.Common.Client;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;

namespace XKit.Lib.Connector.Service {

    /// <summary>
    /// This class provides the base for service clients.  A service client presents
    /// an interface that routes to a service operation, abstracting all the logic
    /// necessary to do the call.  The client is really a proxy.  It prepares the call, 
    /// and the actually routing of the call is handled by an ICallRouter
    /// </summary>

    public class ServiceClientBase {

        public enum LogCodes {
            ServiceCallFailed,
            OperationFailed
        }

        // =====================================================================
        // private
        // =====================================================================
        private readonly IReadOnlyDescriptor Descriptor;
        private readonly IFabricConnector Connector;
        private readonly ServiceCallTypeParameters DefaultCallTypeParameters;
        private readonly ServiceClientErrorHandling DefaultErrorHandling;
        private ServiceCallTypeParameters currentCallTypeParameters;
        private readonly ILogSession defaultLog;
        private ServiceClientErrorHandling? currentErrorHandling;
        private ServiceCallPolicy currentPolicy;
        private ILogSession currentLog; 

        private readonly string TargetHostId;
        private readonly SemaphoreSlim callSynchronizer = new(1, 1);
        private readonly string callInterfaceName;
        
        // =====================================================================
        // construction
        // =====================================================================

        public ServiceClientBase(
            IReadOnlyDescriptor descriptor,
            string callInterfaceName,
            ILogSession log,
            IFabricConnector connector,
            ServiceCallTypeParameters defaultCallTypeParameters = null,
            ServiceClientErrorHandling errorHandling = ServiceClientErrorHandling.LogWarning,
            string targetHostId = null
        ) {
            this.Descriptor = descriptor;
            this.callInterfaceName = callInterfaceName;
            this.DefaultErrorHandling = errorHandling;
            this.defaultLog = log;
            Connector = connector ?? throw new ArgumentNullException(nameof(connector));
            this.DefaultCallTypeParameters = defaultCallTypeParameters ?? ServiceCallTypeParameters.SyncResult();
            this.TargetHostId = targetHostId;
        }

        // =====================================================================
        // protected
        // =====================================================================

        protected virtual IReadOnlyDescriptor ServiceDescriptor => this.Descriptor; 
        protected ServiceClientErrorHandling ErrorHandling => currentErrorHandling ?? DefaultErrorHandling; 
        //protected string CorrelationId => currentCorrelationId ?? ClientParameters.DefaultCorrelationId;
        protected ServiceCallTypeParameters CallTypeParameters => currentCallTypeParameters ?? DefaultCallTypeParameters;
        protected ServiceCallPolicy Policy => currentPolicy;
        protected ILogSession Log => currentLog ?? defaultLog;

        protected async Task<IServiceCallRouter> BeginCall(
            ServiceCallTypeParameters callTypeParameters,
            ILogSession log,
            ServiceClientErrorHandling? errorHandling,
            IServiceCallRouter callRouter,
            ServiceCallPolicy policy
        ) {
            await this.callSynchronizer.WaitAsync();

            this.currentCallTypeParameters = callTypeParameters;
            this.currentLog = log;
            this.currentErrorHandling = errorHandling;
            this.currentPolicy = policy;

            if (callRouter != null) {
                return callRouter;
            }
            return await Connector.CreateCallRouter(
                Descriptor,
                Log,
                ErrorHandling == ServiceClientErrorHandling.ThrowException, 
                allowRegistryRefreshIfRequested: true
            );
        }

        protected async Task<IReadOnlyList<ServiceCallResult>> EndCall(
            Task<IReadOnlyList<ServiceCallResult>> resultTask
        ) {

            ServiceClientErrorHandling errorHandling = ErrorHandling;

            this.currentCallTypeParameters = null;
            this.currentLog = null;
            this.currentErrorHandling = null;
            this.currentPolicy = null;

            callSynchronizer.Release();

            var resultList = await resultTask;

            if (!resultList.Any(r => !r.HasError)) {
                var firstResult = resultList[0];

                var attributes = new {
                    Target = Descriptor,
                    Operation = firstResult.OperationName,
                    Status = firstResult.ServiceCallStatus
                };
                var code = !firstResult.Completed ? LogCodes.ServiceCallFailed : LogCodes.OperationFailed;
                switch(errorHandling) {
                case ServiceClientErrorHandling.DoNothing:
                    break;
                case ServiceClientErrorHandling.LogInfo:
                    Log?.Info(
                        firstResult.Message,
                        attributes: attributes,
                        code : code
                    );
                    break;
                case ServiceClientErrorHandling.LogWarning:
                    Log?.Warning(
                        firstResult.Message,
                        attributes: attributes,
                        code : code
                    );
                    break;
                case ServiceClientErrorHandling.LogError:
                    Log?.Error(
                        firstResult.Message,
                        attributes: attributes,
                        code : code
                    );
                    break;
                case ServiceClientErrorHandling.LogFatality:
                case ServiceClientErrorHandling.ThrowException:
                    Log?.Fatality(
                        firstResult.Message,
                        attributes: attributes
                    );
                    if (errorHandling == ServiceClientErrorHandling.ThrowException) {
                        throw new Exception("Service call failed: " + firstResult.Message);
                    }
                    break;
                }

                return resultList;
            }

            return 
                resultList
                .OrderBy(r => !r.HasError)
                .ThenBy(r => r.Timestamp)
                .ToArray();
        }

        // protected async Task<IReadOnlyList<ServiceCallResult<T>>> EndCallWithResultBody<T>(
        //     ServiceCallResult<T> result
        // ) where T : class => (await EndCall(Task.FromResult((IReadOnlyList<ServiceCallResult<T>>) new[] { result }))).Select(r => r.ConvertTo<T>()).ToArray();

        // --------------------------------------------------------------------------

        protected async Task<ServiceCallResult<TResponseBody>> ExecuteCall<TRequestBody, TResponseBody>(
            TRequestBody requestBody,
            ServiceCallTypeParameters callTypeParameters = null,
            [CallerMemberName] string callerMemberNameAsOperationName = ""
        ) where TRequestBody : class where TResponseBody : class 
            => (await ExecuteCallEx<TRequestBody, TResponseBody>(
                callerMemberNameAsOperationName,
                requestBody,
                callTypeParameters
            ))[0];

        protected async Task<ServiceCallResult<TResponseBody>> ExecuteCall<TResponseBody>(
            ServiceCallTypeParameters callTypeParameters = null,
            [CallerMemberName] string callerMemberNameAsOperationName = ""
        ) where TResponseBody : class 
            => (await ExecuteCallEx<TResponseBody>(
                callerMemberNameAsOperationName,
                callTypeParameters
            ))[0];

        protected async Task<ServiceCallResult> ExecuteCall<TRequestBody>(
            TRequestBody requestBody,
            ServiceCallTypeParameters callTypeParameters = null,
            [CallerMemberName] string callerMemberNameAsOperationName = ""
        ) where TRequestBody : class  
            => (await ExecuteCallEx<TRequestBody>(
                callerMemberNameAsOperationName,
                requestBody,
                callTypeParameters
            ))[0];

        protected async Task<ServiceCallResult> ExecuteCall(
            ServiceCallTypeParameters callTypeParameters = null,
            [CallerMemberName] string callerMemberNameAsOperationName = ""
        ) => (await ExecuteCallEx(
                callerMemberNameAsOperationName,
                callTypeParameters
            ))[0];

        // --------------------------------------------------------------------------

        protected async Task<IReadOnlyList<ServiceCallResult<TResponseBody>>> ExecuteCallEx<TRequestBody, TResponseBody>(
            string operationName,
            TRequestBody requestBody,
            ServiceCallTypeParameters callTypeParameters,
            ILogSession log = null,
            IServiceCallRouter callRouter = null,
            ServiceClientErrorHandling? errorHandling = null,
            ServiceCallPolicy policy = null
        ) where TRequestBody : class where TResponseBody : class {

            var router = await BeginCall(
                callTypeParameters,
                log,
                errorHandling,
                callRouter, 
                policy
            );

            if (router == null) {
                return (await EndCall(
                    Task.FromResult(
                        (IReadOnlyList<ServiceCallResult>) new[] { 
                            HydrateResult(
                                new ServiceCallResult(),
                                ServiceCallStatusEnum.NotFound,
                                null,
                                LogResultStatusEnum.Incomplete
                            )
                        }
                    )
                )).Select(r => r.ConvertTo<TResponseBody>()).ToArray();
            }

            var request = CreateRequest(
                operationName: operationName,
                requestBody: requestBody
            );
            return (await EndCall(
                router.ExecuteCall(request, Log, Policy, TargetHostId)
            )).Select(r => r.ConvertTo<TResponseBody>()).ToArray();
        }

        protected async Task<IReadOnlyList<ServiceCallResult<TResponseBody>>> ExecuteCallEx<TResponseBody>(
            string operationName,
            ServiceCallTypeParameters callTypeParameters,
            ILogSession log = null,
            IServiceCallRouter callRouter = null,
            ServiceClientErrorHandling? errorHandling = null,
            ServiceCallPolicy policy = null,
            string requestJsonPayload = null
        ) where TResponseBody : class {

            var router = await BeginCall(
                callTypeParameters,
                log,
                errorHandling,
                callRouter,
                policy
            );

            if (router == null) {
                return (await EndCall(
                    Task.FromResult(
                        (IReadOnlyList<ServiceCallResult>) new[] { 
                            HydrateResult(
                                new ServiceCallResult(),
                                ServiceCallStatusEnum.NotFound,
                                null,
                                LogResultStatusEnum.Incomplete
                            )
                        }
                    )
                )).Select(r => r.ConvertTo<TResponseBody>()).ToArray();
            }

            var request = CreateRequest(
                operationName: operationName, 
                jsonPayload: requestJsonPayload
            );
            return (await EndCall(
                router.ExecuteCall(request, Log, Policy, TargetHostId)
            )).Select(r => r.ConvertTo<TResponseBody>()).ToArray();
        }

        protected async Task<IReadOnlyList<ServiceCallResult>> ExecuteCallEx<TRequestBody>(
            string operationName,
            TRequestBody requestBody,
            ServiceCallTypeParameters callTypeParameters,
            ILogSession log = null,
            IServiceCallRouter callRouter = null,
            ServiceClientErrorHandling? errorHandling = null,
            ServiceCallPolicy policy = null
        ) where TRequestBody : class {

            var router = await BeginCall(
                callTypeParameters,
                log,
                errorHandling,
                callRouter,
                policy
            );

            if (router == null) {
                var response = await EndCall(
                    Task.FromResult(
                        (IReadOnlyList<ServiceCallResult>)new[] { 
                            HydrateResult(
                                new ServiceCallResult(),
                                ServiceCallStatusEnum.NotFound,
                                null,
                                LogResultStatusEnum.Incomplete
                            )
                        }
                    )
                );
                return response;
            }

            var request = CreateRequest(
                operationName: operationName,
                requestBody: requestBody
            );
            return await EndCall(
                router.ExecuteCall(request, Log, Policy, TargetHostId)
            );
        }

        protected async Task<IReadOnlyList<ServiceCallResult>> ExecuteCallEx(
            string operationName,
            ServiceCallTypeParameters callTypeParameters,
            ILogSession log = null,
            IServiceCallRouter callRouter = null,
            ServiceClientErrorHandling? errorHandling = null,
            ServiceCallPolicy policy = null,
            string requestJsonPayload = null
        ) {

            var router = await BeginCall(
                callTypeParameters,
                log,
                errorHandling,
                callRouter,
                policy
            );

            if (router == null) {
                return await EndCall(
                    Task.FromResult(
                        (IReadOnlyList<ServiceCallResult>)new[] { 
                            HydrateResult(
                                new ServiceCallResult(),
                                ServiceCallStatusEnum.NotFound,
                                null,
                                LogResultStatusEnum.Incomplete
                            )
                        }
                    )
                );
            }

            var request = CreateRequest(
                operationName: operationName, 
                jsonPayload: requestJsonPayload
            );
            return await EndCall(
                router.ExecuteCall(request, Log, Policy, TargetHostId)
            );
        }

        // =====================================================================
        // private
        // =====================================================================

        private ServiceCallRequest<TRequestBody> CreateRequest<TRequestBody>(
            string operationName,
            TRequestBody requestBody
        ) where TRequestBody : class => ServiceCallRequest<TRequestBody>.Create(
            callTypeParameters: CallTypeParameters, 
            correlationId: Log?.CorrelationId, 
            operationName: this.callInterfaceName == null || operationName.Contains('.') ? operationName : $"{this.callInterfaceName}.{operationName}", 
            requestBody: requestBody,
            requestorFabricId: Log?.OriginatorFabricId,
            requestorInstanceId: Log?.OriginatorInstanceId
        );

        private ServiceCallRequest CreateRequest(
            string operationName,
            string jsonPayload
        ) => ServiceCallRequest.Create(
            payload: jsonPayload,
            callTypeParameters: CallTypeParameters, 
            correlationId: Log?.CorrelationId, 
            operationName: this.callInterfaceName == null || operationName.Contains('.') ? operationName : $"{this.callInterfaceName}.{operationName}",
            requestorFabricId: Log?.OriginatorFabricId,
            requestorInstanceId: Log?.OriginatorInstanceId
        );

        private ServiceCallResult HydrateResult(
            ServiceCallResult resultBase,
            ServiceCallStatusEnum callStatus,
            ServiceCallRequest request,
            LogResultStatusEnum operationStatus,
            string message = null
        ) {
            resultBase.ServiceCallStatus = callStatus;
            resultBase.CorrelationId = request?.CorrelationId;
            resultBase.Message = message;
            resultBase.OperationName = request?.OperationName;
            resultBase.OperationStatus = operationStatus;
            resultBase.Code = resultBase?.Code;
            resultBase.RequestorFabricId = request?.RequestorFabricId;
            resultBase.RequestorInstanceId = request?.RequestorInstanceId;
            resultBase.Service = Descriptor.Clone();
            resultBase.ServiceStatus = null;
            resultBase.ResponderInstanceId = null;
            resultBase.ResponderFabricId = null;
            resultBase.OperationId = Guid.Empty;
            resultBase.Timestamp = DateTime.UtcNow;
            return resultBase;
        }
    }

    public class ServiceClientBase<TCallInterface> : ServiceClientBase, IServiceClient<TCallInterface>
        where TCallInterface : IServiceApi {

        // =====================================================================
        // construction
        // =====================================================================

        public ServiceClientBase(
            IReadOnlyDescriptor descriptor,
            ILogSession log,
            IFabricConnector connector,
            ServiceCallTypeParameters defaultCallTypeParameters = null,
            ServiceClientErrorHandling errorHandling = ServiceClientErrorHandling.LogWarning,
            string targetHostId = null
        ) : base(
            descriptor,
            typeof(TCallInterface).Name,
            log,
            connector,
            defaultCallTypeParameters,
            errorHandling,
            targetHostId
        ) { }

        // --------------------------------------------------------------------------


        // =====================================================================
        // IServiceClient
        // =====================================================================

        Task<IReadOnlyList<ServiceCallResult<TResponseBody>>> IServiceClient<TCallInterface>.ExecuteWith<TRequestBody, TResponseBody>(
            Expression<Func<TCallInterface, Task<ServiceCallResult<TResponseBody>>>> expression,
            TRequestBody requestBody, 
            ServiceCallTypeParameters callTypeParameters, 
            ILogSession log, 
            IServiceCallRouter callRouter,
            ServiceClientErrorHandling? errorHandling,
            string requestJsonPayload
        ) => ExecuteCallEx<TRequestBody, TResponseBody>(
                operationName: ((MethodCallExpression)expression.Body).Method.Name,
                requestBody: requestBody,
                callTypeParameters: callTypeParameters,
                log: log,
                callRouter: callRouter,
                errorHandling: errorHandling
            );

        Task<IReadOnlyList<ServiceCallResult>> IServiceClient<TCallInterface>.ExecuteWith<TRequestBody>(
            Expression<Func<TCallInterface, Task<ServiceCallResult>>> expression,
            TRequestBody requestBody, 
            ServiceCallTypeParameters callTypeParameters, 
            ILogSession log, 
            IServiceCallRouter callRouter,
            ServiceClientErrorHandling? errorHandling,
            string requestJsonPayload
        ) => ExecuteCallEx<TRequestBody>(
                operationName: ((MethodCallExpression)expression.Body).Method.Name,
                requestBody: requestBody,
                callTypeParameters: callTypeParameters,
                log: log,
                callRouter: callRouter,
                errorHandling: errorHandling
            );

        Task<IReadOnlyList<ServiceCallResult<TResponseBody>>> IServiceClient<TCallInterface>.ExecuteWith<TResponseBody>(
            Expression<Func<TCallInterface, Task<ServiceCallResult<TResponseBody>>>> expression,
            ServiceCallTypeParameters callTypeParameters, 
            ILogSession log, 
            IServiceCallRouter callRouter,
            ServiceClientErrorHandling? errorHandling
        ) => ExecuteCallEx<TResponseBody>(
                operationName: ((MethodCallExpression)expression.Body).Method.Name,
                callTypeParameters: callTypeParameters,
                log: log,
                callRouter: callRouter,
                errorHandling: errorHandling
            );

        Task<IReadOnlyList<ServiceCallResult>> IServiceClient<TCallInterface>.ExecuteWith(
            Expression<Func<TCallInterface, Task<ServiceCallResult>>> expression,
            ServiceCallTypeParameters callTypeParameters, 
            ILogSession log, 
            IServiceCallRouter callRouter,
            ServiceClientErrorHandling? errorHandling
        ) => ExecuteCallEx(
                operationName: ((MethodCallExpression)expression.Body).Method.Name,
                callTypeParameters: callTypeParameters,
                log: log,
                callRouter: callRouter,
                errorHandling: errorHandling
            );
    }
}

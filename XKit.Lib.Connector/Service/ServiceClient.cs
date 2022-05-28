using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XKit.Lib.Common.Client;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;

namespace XKit.Lib.Connector.Service {

    public class ServiceClient : ServiceClientBase, IGenericServiceClient {

        // =====================================================================
        // construction
        // =====================================================================

        public ServiceClient(
            IReadOnlyDescriptor descriptor,
            string operationInterfaceName,
            ILogSession log,
            IDependencyConnector connector = null,
            ServiceCallTypeParameters defaultCallTypeParameters = null,
            ServiceClientErrorHandling errorHandling = ServiceClientErrorHandling.LogWarning,
            string targetHostId = null
        ) : base(
            descriptor,
            operationInterfaceName,
            log,
            connector,
            defaultCallTypeParameters,
            errorHandling,
            targetHostId
        ) { }

        // =====================================================================
        // IGenericServiceClient
        // =====================================================================

        Task<IReadOnlyList<ServiceCallResult<TResponseBody>>> IGenericServiceClient.ExecuteCall<TRequestBody, TResponseBody>(
            string operationName, 
            TRequestBody requestBody, 
            ServiceCallTypeParameters callTypeParameters, 
            ILogSession log,
            ServiceClientErrorHandling? errorHandling,
            ServiceCallPolicy policy
        ) {
            return ExecuteCallEx<TRequestBody, TResponseBody>(
                operationName: operationName,
                requestBody: requestBody,
                callTypeParameters: callTypeParameters,
                log: log,
                callRouter: null,
                errorHandling: errorHandling,
                policy: policy
            );
        }

        Task<IReadOnlyList<ServiceCallResult<TResponseBody>>> IGenericServiceClient.ExecuteCall<TResponseBody>(
            string operationName, 
            ServiceCallTypeParameters callTypeParameters, 
            ILogSession log,
            ServiceClientErrorHandling? errorHandling,
            string requestJsonPayload,
            ServiceCallPolicy policy
        ) {
            return ExecuteCallEx<TResponseBody>(
                operationName: operationName,
                requestJsonPayload: requestJsonPayload,
                callTypeParameters: callTypeParameters,
                log: log,
                callRouter: null,
                errorHandling: errorHandling,
                policy: policy
            );
        }

        Task<IReadOnlyList<ServiceCallResult>> IGenericServiceClient.ExecuteCall<TRequestBody>(
            string operationName, 
            TRequestBody requestBody, 
            ServiceCallTypeParameters callTypeParameters, 
            ILogSession log,
            ServiceClientErrorHandling? errorHandling,
            ServiceCallPolicy policy
        ) {
            return ExecuteCallEx(
                operationName: operationName,
                requestBody: requestBody,
                callTypeParameters: callTypeParameters,
                log: log,
                callRouter: null,
                errorHandling: errorHandling,
                policy: policy
            );
        }

        Task<IReadOnlyList<ServiceCallResult>> IGenericServiceClient.ExecuteCall(
            string operationName, 
            ServiceCallTypeParameters callTypeParameters, 
            ILogSession log,
            ServiceClientErrorHandling? errorHandling,
            string requestJsonPayload,
            ServiceCallPolicy policy
        ) {
            return ExecuteCallEx(
                operationName: operationName,
                requestJsonPayload: requestJsonPayload,
                callTypeParameters: callTypeParameters,
                log: log,
                callRouter: null,
                errorHandling: errorHandling,
                policy: policy
            );
        }
    }
}

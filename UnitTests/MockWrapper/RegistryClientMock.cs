using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using XKit.Lib.Common.Client;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Services.Registry;
using XKit.Lib.Connector.Fabric;

namespace UnitTests.MockWrapper {

    public class RegistryClientMock : IRegistryClient {

        private ServiceTopologyMap registerResponse;
        private ServiceTopologyMap refreshResponse;

        // =====================================================================
        // Setup
        // =====================================================================

        public void Setup_Register(ServiceTopologyMap response) {
            this.registerResponse = response;
        }

        public void Setup_Refresh(ServiceTopologyMap response) {
            this.refreshResponse = response;
        }

        Task<IReadOnlyList<ServiceCallResult<TResponseBody>>> IServiceClient<IRegistryApi>.ExecuteWith<TRequestBody, TResponseBody>(
            Expression<Func<IRegistryApi, 
            Task<ServiceCallResult<TResponseBody>>>> expression, 
            TRequestBody requestBody, 
            ServiceCallTypeParameters callTypeParameters, 
            ILogSession log, 
            IServiceCallRouter callRouter, 
            ServiceClientErrorHandling? errorHandling
        ) {
            throw new Exception("Mock not implemented");
        }

        Task<IReadOnlyList<ServiceCallResult<TResponseBody>>> IServiceClient<IRegistryApi>.ExecuteWith<TResponseBody>(
            Expression<Func<IRegistryApi, Task<ServiceCallResult<TResponseBody>>>> expression, 
            ServiceCallTypeParameters callTypeParameters, 
            ILogSession log, 
            IServiceCallRouter callRouter, 
            ServiceClientErrorHandling? errorHandling
        ) {
            throw new Exception("Mock not implemented");
        }

        Task<IReadOnlyList<ServiceCallResult>> IServiceClient<IRegistryApi>.ExecuteWith<TRequestBody>(
            Expression<Func<IRegistryApi, Task<ServiceCallResult>>> expression, 
            TRequestBody requestBody, 
            ServiceCallTypeParameters callTypeParameters, 
            ILogSession log, 
            IServiceCallRouter callRouter, 
            ServiceClientErrorHandling? errorHandling
        ) {
            throw new Exception("Mock not implemented");
        }

        Task<IReadOnlyList<ServiceCallResult>> IServiceClient<IRegistryApi>.ExecuteWith(
            Expression<Func<IRegistryApi, Task<ServiceCallResult>>> expression, 
            ServiceCallTypeParameters callTypeParameters, 
            ILogSession log, 
            IServiceCallRouter callRouter, 
            ServiceClientErrorHandling? errorHandling
        ) {
            throw new Exception("Mock not implemented");
        }

        Task<ServiceCallResult<ServiceTopologyMap>> IRegistryApi.Register(
            FabricRegistration request
        ) {
            return Task.FromResult(                
                new ServiceCallResult<ServiceTopologyMap> {
                    ServiceCallStatus = ServiceCallStatusEnum.Completed,
                    OperationStatus = registerResponse == null ? LogResultStatusEnum.NonRetriableError : LogResultStatusEnum.Success,
                    ResponseBody = registerResponse
                }
            );
        }

        Task<ServiceCallResult<ServiceTopologyMap>> IRegistryApi.Refresh(RefreshRegistrationRequest request) {
            return Task.FromResult(
                new ServiceCallResult<ServiceTopologyMap> {
                    ServiceCallStatus = ServiceCallStatusEnum.Completed,
                    OperationStatus = registerResponse == null ? LogResultStatusEnum.NonRetriableError : LogResultStatusEnum.Success,
                    ResponseBody = refreshResponse
                }
            );
        }

        Task<ServiceCallResult> IRegistryApi.Unregister(UnregisterRequest request) {
            return Task.FromResult(
                new ServiceCallResult {
                    ServiceCallStatus = ServiceCallStatusEnum.Completed,
                    OperationStatus = LogResultStatusEnum.Success,
                }
            );
        }

        Task<IReadOnlyList<ServiceCallResult<TResponseBody>>> IServiceClient<IRegistryApi>.ExecuteWith<TRequestBody, TResponseBody>(string operationName, TRequestBody requestBody, ServiceCallTypeParameters callTypeParameters, ILogSession log, IServiceCallRouter callRouter, ServiceClientErrorHandling? errorHandling) {
            throw new Exception("Mock not implemented");
        }

        Task<IReadOnlyList<ServiceCallResult<TResponseBody>>> IServiceClient<IRegistryApi>.ExecuteWith<TResponseBody>(string operationName, string requestJsonPayload, ServiceCallTypeParameters callTypeParameters, ILogSession log, IServiceCallRouter callRouter, ServiceClientErrorHandling? errorHandling) {
            throw new Exception("Mock not implemented");
        }

        Task<IReadOnlyList<ServiceCallResult>> IServiceClient<IRegistryApi>.ExecuteWith<TRequestBody>(string operationName, TRequestBody requestBody, ServiceCallTypeParameters callTypeParameters, ILogSession log, IServiceCallRouter callRouter, ServiceClientErrorHandling? errorHandling) {
            throw new Exception("Mock not implemented");
        }

        Task<IReadOnlyList<ServiceCallResult>> IServiceClient<IRegistryApi>.ExecuteWith(string operationName, string requestJsonPayload, ServiceCallTypeParameters callTypeParameters, ILogSession log, IServiceCallRouter callRouter, ServiceClientErrorHandling? errorHandling) {
            throw new Exception("Mock not implemented");
        }
    }
}

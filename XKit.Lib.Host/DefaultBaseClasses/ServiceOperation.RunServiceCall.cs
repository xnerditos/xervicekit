using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Log;

namespace XKit.Lib.Host.DefaultBaseClasses {
    
    public partial class ServiceOperation {

        public enum BaseMonitorCodes { 
            CouldNotObtainCallbackRouterForOperation,
            FailedInit
        }

        // ---------------------------------------------------------------------
        // RunServiceCall<TRequestBody, TResponseBody> implementations
        // ---------------------------------------------------------------------

        /// <summary>
        /// Runs async operation logic in a lambda.  The lambda must return from a call to End().
        /// </summary>
        /// <param name="request.RequestBody"></param>
        /// <param name="operationAction"></param>
        /// <returns></returns>
        protected Task<ServiceCallResult<TResponseBody>> RunServiceCall<TRequestBody, TResponseBody>(
            TRequestBody requestBody, 
            Func<TRequestBody, Task<OperationResult<TResponseBody>>> operationAction,
            Func<TRequestBody, bool> requestValidationAction = null,
            Func<TRequestBody, Task<bool>> preCallAction = null,
            Func<OperationResult<TResponseBody>, Task> postCallAction = null,
            bool? isLongRunning = null,
            LogOptionsEnum loggingOptions = LogOptionsEnum.Default,
            [CallerMemberName] string callerMethod = null
        ) where TRequestBody : class where TResponseBody : class 
            => RunServiceCallAs<TRequestBody, TResponseBody>(
                operationName: callerMethod,
                requestBody: requestBody,
                operationAction: operationAction,
                requestValidationAction: requestValidationAction,
                preCallAction: preCallAction,
                postCallAction: postCallAction,
                isLongRunning: isLongRunning,
                loggingOptions: loggingOptions
            );

        /// <summary>
        /// Runs async operation logic in a lambda.  The lambda must call simply return the result.
        /// </summary>
        /// <param name="request.RequestBody"></param>
        /// <param name="operationAction"></param>
        /// <returns></returns>
        protected Task<ServiceCallResult<TResponseBody>> RunServiceCall<TRequestBody, TResponseBody>(
            TRequestBody requestBody, 
            Func<TRequestBody, Task<TResponseBody>> operationAction,
            Func<TRequestBody, bool> requestValidationAction = null,
            Func<TRequestBody, Task<bool>> preCallAction = null,
            Func<OperationResult<TResponseBody>, Task> postCallAction = null,
            bool? isLongRunning = null,
            LogOptionsEnum loggingOptions = LogOptionsEnum.Default,
            [CallerMemberName] string callerMethod = null
        ) where TRequestBody : class where TResponseBody : class 
            => RunServiceCallAs<TRequestBody, TResponseBody>(
                operationName: callerMethod,
                requestBody: requestBody,
                operationAction: operationAction,
                requestValidationAction: requestValidationAction,
                preCallAction: preCallAction,
                postCallAction: postCallAction,
                isLongRunning: isLongRunning,
                loggingOptions: loggingOptions
            );

        // ---------------------------------------------------------------------
        // RunServiceCall<TResponseBody> implementations
        // ---------------------------------------------------------------------

        /// <summary>
        /// Runs async operation logic in a lambda.  The lambda must return from a call to End().
        /// </summary>
        /// <param name="request.RequestBody"></param>
        /// <param name="operationAction"></param>
        /// <returns></returns>
        protected Task<ServiceCallResult<TResponseBody>> RunServiceCall<TResponseBody>(
            Func<Task<OperationResult<TResponseBody>>> operationAction,
            Func<Task<bool>> preCallAction = null,
            Func<OperationResult<TResponseBody>, Task> postCallAction = null,
            bool? isLongRunning = null,
            LogOptionsEnum loggingOptions = LogOptionsEnum.Default,
            [CallerMemberName] string callerMethod = null
        ) where TResponseBody : class  
            => RunServiceCallAs<TResponseBody>(
                operationName: callerMethod,
                operationAction: operationAction,
                preCallAction: preCallAction,
                postCallAction: postCallAction,
                isLongRunning: isLongRunning,
                loggingOptions: loggingOptions
            );

        /// <summary>
        /// Runs async operation logic in a lambda.  The lambda must call simply return the result.
        /// </summary>
        /// <param name="request.RequestBody"></param>
        /// <param name="operationAction"></param>
        /// <returns></returns>
        protected Task<ServiceCallResult<TResponseBody>> RunServiceCall<TResponseBody>(
            Func<Task<TResponseBody>> operationAction,
            Func<Task<bool>> preCallAction = null,
            Func<OperationResult<TResponseBody>, Task> postCallAction = null,
            bool? isLongRunning = null,
            LogOptionsEnum loggingOptions = LogOptionsEnum.Default,
            [CallerMemberName] string callerMethod = null
        ) where TResponseBody : class  
            => RunServiceCallAs<TResponseBody>(
                operationName: callerMethod,
                operationAction: operationAction,
                preCallAction: preCallAction,
                postCallAction: postCallAction,
                isLongRunning: isLongRunning,
                loggingOptions: loggingOptions
            );

        // ---------------------------------------------------------------------
        // RunServiceCall<TRequestBody> implementations
        // ---------------------------------------------------------------------

        /// <summary> 
        /// Runs async operation logic in a lambda.  The lambda returns no result.
        /// </summary>
        /// <param name="RequestBody"></param>
        /// <param name="operationAction"></param>
        /// <returns></returns>
        protected Task<ServiceCallResult> RunServiceCall<TRequestBody>(
            TRequestBody requestBody, 
            Func<TRequestBody, Task<OperationResult>> operationAction,
            Func<TRequestBody, bool> requestValidationAction = null,
            Func<TRequestBody, Task<bool>> preCallAction = null,
            Func<OperationResult, Task> postCallAction = null,
            bool? isLongRunning = null,
            LogOptionsEnum loggingOptions = LogOptionsEnum.Default,
            [CallerMemberName] string callerMethod = null
        ) where TRequestBody : class  
            => RunServiceCallAs<TRequestBody>(
                operationName: callerMethod,
                requestBody: requestBody,
                operationAction: operationAction,
                requestValidationAction: requestValidationAction,
                preCallAction: preCallAction,
                postCallAction: postCallAction,
                isLongRunning: isLongRunning,
                loggingOptions: loggingOptions
            );

        /// <summary> 
        /// Runs async operation logic in a lambda.  The lambda returns no result.
        /// </summary>
        /// <param name="RequestBody"></param>
        /// <param name="operationAction"></param>
        /// <returns></returns>
        protected Task<ServiceCallResult> RunServiceCall<TRequestBody>(
            TRequestBody requestBody, 
            Func<TRequestBody, Task> operationAction,
            Func<TRequestBody, bool> requestValidationAction = null,
            Func<TRequestBody, Task<bool>> preCallAction = null,
            Func<OperationResult, Task> postCallAction = null,
            bool? isLongRunning = null,
            LogOptionsEnum loggingOptions = LogOptionsEnum.Default,
            [CallerMemberName] string callerMethod = null
        ) where TRequestBody : class 
            => RunServiceCallAs<TRequestBody>(
                operationName: callerMethod,
                requestBody: requestBody,
                operationAction: operationAction,
                requestValidationAction: requestValidationAction,
                preCallAction: preCallAction,
                postCallAction: postCallAction,
                isLongRunning: isLongRunning,
                loggingOptions: loggingOptions
            );

        // ---------------------------------------------------------------------
        // RunServiceCall<> implementations
        // ---------------------------------------------------------------------

        /// <summary> 
        /// Runs async operation logic in a lambda.  The lambda returns no result.
        /// </summary>
        /// <param name="operationAction"></param>
        /// <returns></returns>
        protected Task<ServiceCallResult> RunServiceCall(
            Func<Task<OperationResult>> operationAction,
            Func<Task<bool>> preCallAction = null,
            Func<OperationResult, Task> postCallAction = null,
            bool? isLongRunning = null,
            LogOptionsEnum loggingOptions = LogOptionsEnum.Default,
            [CallerMemberName] string callerMethod = null
        ) => RunServiceCallAs(
                operationName: callerMethod,
                operationAction: operationAction,
                preCallAction: preCallAction,
                postCallAction: postCallAction,
                isLongRunning: isLongRunning,
                loggingOptions: loggingOptions
            );

        /// <summary> 
        /// Runs async operation logic in a lambda.  The lambda returns no result.
        /// </summary>
        /// <param name="RequestBody"></param>
        /// <param name="operationAction"></param>
        /// <returns></returns>
        protected Task<ServiceCallResult> RunServiceCall(
            Func<Task> operationAction,
            Func<Task<bool>> preCallAction = null,
            Func<OperationResult, Task> postCallAction = null,
            bool? isLongRunning = null,
            LogOptionsEnum loggingOptions = LogOptionsEnum.Default,
            [CallerMemberName] string callerMethod = null
        ) => RunServiceCallAs(
                operationName: callerMethod,
                operationAction: operationAction,
                preCallAction: preCallAction,
                postCallAction: postCallAction,
                isLongRunning: isLongRunning,
                loggingOptions: loggingOptions
            );

        // ---------------------------------------------------------------------
        // RunServiceCallAs<TRequestBody, TResponseBody> implementations
        // ---------------------------------------------------------------------

        /// <summary>
        /// Runs async operation logic in a lambda.  The lambda must return from a call to End().
        /// </summary>
        /// <param name="request.RequestBody"></param>
        /// <param name="operationAction"></param>
        /// <returns></returns>
        protected async Task<ServiceCallResult<TResponseBody>> RunServiceCallAs<TRequestBody, TResponseBody>(
            string operationName,
            TRequestBody requestBody, 
            Func<TRequestBody, Task<OperationResult<TResponseBody>>> operationAction,
            Func<TRequestBody, bool> requestValidationAction = null,
            Func<TRequestBody, Task<bool>> preCallAction = null,
            Func<OperationResult<TResponseBody>, Task> postCallAction = null,
            bool? isLongRunning = null,
            LogOptionsEnum loggingOptions = LogOptionsEnum.Default
        ) where TRequestBody : class where TResponseBody : class {

            var immediateOperationResult = await base.RunOperation<TRequestBody, TResponseBody>(
                operationName: operationName,
                workItem: requestBody,
                runSynchronous: IsOperationSynchronous,
                operationAction: operationAction,
                workItemValidationAction: requestValidationAction,
                initAction: async () => await InitOperation(),
                preOperationAction: async (req) => await PreOperation(req, preCallAction),
                postOperationAction: async (req, jr) => await PostOperation(jr, postCallAction),
                isLongRunning: isLongRunning,
                loggingOptions: loggingOptions
            );
            return CreateServiceCallResult<TResponseBody>(immediateOperationResult);
        }

        /// <summary>
        /// Runs async operation logic in a lambda.  The lambda must call simply return the result.
        /// </summary>
        /// <param name="request.RequestBody"></param>
        /// <param name="operationAction"></param>
        /// <returns></returns>
        protected async Task<ServiceCallResult<TResponseBody>> RunServiceCallAs<TRequestBody, TResponseBody>(
            string operationName,
            TRequestBody requestBody, 
            Func<TRequestBody, Task<TResponseBody>> operationAction,
            Func<TRequestBody, bool> requestValidationAction = null,
            Func<TRequestBody, Task<bool>> preCallAction = null,
            Func<OperationResult<TResponseBody>, Task> postCallAction = null,
            bool? isLongRunning = null,
            LogOptionsEnum loggingOptions = LogOptionsEnum.Default
        ) where TRequestBody : class where TResponseBody : class {
            
            var immediateOperationResult = await base.RunOperation<TRequestBody, TResponseBody>(
                operationName: operationName,
                workItem: requestBody,
                runSynchronous: IsOperationSynchronous,
                operationAction: operationAction,
                workItemValidationAction: requestValidationAction,
                initAction: async () => await InitOperation(),
                preOperationAction: async (req) => await PreOperation(req, preCallAction),
                postOperationAction: async (req, jr) => await PostOperation(jr, postCallAction),
                isLongRunning: isLongRunning,
                loggingOptions: loggingOptions
            );
            return CreateServiceCallResult<TResponseBody>(immediateOperationResult);
        }

        // ---------------------------------------------------------------------
        // RunServiceCallAs<TResponseBody> implementations
        // ---------------------------------------------------------------------

        /// <summary>
        /// Runs async operation logic in a lambda.  The lambda must return from a call to End().
        /// </summary>
        /// <param name="request.RequestBody"></param>
        /// <param name="operationAction"></param>
        /// <returns></returns>
        protected async Task<ServiceCallResult<TResponseBody>> RunServiceCallAs<TResponseBody>(
            string operationName,
            Func<Task<OperationResult<TResponseBody>>> operationAction,
            Func<Task<bool>> preCallAction = null,
            Func<OperationResult<TResponseBody>, Task> postCallAction = null,
            bool? isLongRunning = null,
            LogOptionsEnum loggingOptions = LogOptionsEnum.Default
        ) where TResponseBody : class {
            
            var immediateOperationResult = await base.RunOperation<TResponseBody>(
                operationName: operationName,
                runSynchronous: IsOperationSynchronous,
                operationAction: operationAction,
                initAction: async () => await InitOperation(),
                preOperationAction: async () => await PreOperation(preCallAction),
                postOperationAction: async (jr) => await PostOperation(jr, postCallAction),
                isLongRunning: isLongRunning,
                loggingOptions: loggingOptions
            );
            return CreateServiceCallResult<TResponseBody>(immediateOperationResult);
        }

        /// <summary>
        /// Runs async operation logic in a lambda.  The lambda must call simply return the result.
        /// </summary>
        /// <param name="request.RequestBody"></param>
        /// <param name="operationAction"></param>
        /// <returns></returns>
        protected async Task<ServiceCallResult<TResponseBody>> RunServiceCallAs<TResponseBody>(
            string operationName,
            Func<Task<TResponseBody>> operationAction,
            Func<Task<bool>> preCallAction = null,
            Func<OperationResult<TResponseBody>, Task> postCallAction = null,
            bool? isLongRunning = null,
            LogOptionsEnum loggingOptions = LogOptionsEnum.Default
        ) where TResponseBody : class {
            
            var immediateOperationResult = await base.RunOperation<TResponseBody>(
                operationName: operationName,
                runSynchronous: IsOperationSynchronous,
                operationAction: operationAction,
                initAction: async () => await InitOperation(),
                preOperationAction: async () => await PreOperation(preCallAction),
                postOperationAction: async (jr) => await PostOperation(jr, postCallAction),
                isLongRunning: isLongRunning,
                loggingOptions: loggingOptions
            );
            return CreateServiceCallResult<TResponseBody>(immediateOperationResult);
        }

        // ---------------------------------------------------------------------
        // RunServiceCallAs<TRequestBody> implementations
        // ---------------------------------------------------------------------

        /// <summary> 
        /// Runs async operation logic in a lambda.  The lambda returns no result.
        /// </summary>
        /// <param name="RequestBody"></param>
        /// <param name="operationAction"></param>
        /// <returns></returns>
        protected async Task<ServiceCallResult> RunServiceCallAs<TRequestBody>(
            string operationName,
            TRequestBody requestBody, 
            Func<TRequestBody, Task<OperationResult>> operationAction,
            Func<TRequestBody, bool> requestValidationAction = null,
            Func<TRequestBody, Task<bool>> preCallAction = null,
            Func<OperationResult, Task> postCallAction = null,
            bool? isLongRunning = null,
            LogOptionsEnum loggingOptions = LogOptionsEnum.Default
        ) where TRequestBody : class {
            
            var immediateOperationResult = await base.RunOperation<TRequestBody>(
                operationName: operationName,
                workItem: requestBody,
                runSynchronous: IsOperationSynchronous,
                operationAction: operationAction,
                workItemValidationAction: requestValidationAction,
                initAction: async () => await InitOperation(),
                preOperationAction: async (req) => await PreOperation(req, preCallAction),
                postOperationAction: async (req, jr) => await PostOperation(jr, postCallAction),
                isLongRunning: isLongRunning,
                loggingOptions: loggingOptions
            );
            return CreateServiceCallResult(immediateOperationResult);
        }

        /// <summary> 
        /// Runs async operation logic in a lambda.  The lambda returns no result.
        /// </summary>
        /// <param name="RequestBody"></param>
        /// <param name="operationAction"></param>
        /// <returns></returns>
        protected async Task<ServiceCallResult> RunServiceCallAs<TRequestBody>(
            string operationName,
            TRequestBody requestBody, 
            Func<TRequestBody, Task> operationAction,
            Func<TRequestBody, bool> requestValidationAction = null,
            Func<TRequestBody, Task<bool>> preCallAction = null,
            Func<OperationResult, Task> postCallAction = null,
            bool? isLongRunning = null,
            LogOptionsEnum loggingOptions = LogOptionsEnum.Default
        ) where TRequestBody : class {
            
            var immediateOperationResult = await base.RunOperation<TRequestBody>(
                operationName: operationName,
                workItem: requestBody,
                runSynchronous: IsOperationSynchronous,
                operationAction: operationAction,
                workItemValidationAction: requestValidationAction,
                initAction: async () => await InitOperation(),
                preOperationAction: async (req) => await PreOperation(req, preCallAction),
                postOperationAction: async (req, jr) => await PostOperation(jr, postCallAction),
                isLongRunning: isLongRunning,
                loggingOptions: loggingOptions
            );
            return CreateServiceCallResult(immediateOperationResult);
        }

        // ---------------------------------------------------------------------
        // RunServiceCallAs<> implementations
        // ---------------------------------------------------------------------

        /// <summary> 
        /// Runs async operation logic in a lambda.  The lambda returns no result.
        /// </summary>
        /// <param name="operationAction"></param>
        /// <returns></returns>
        protected async Task<ServiceCallResult> RunServiceCallAs(
            string operationName,
            Func<Task<OperationResult>> operationAction,
            Func<Task<bool>> preCallAction = null,
            Func<OperationResult, Task> postCallAction = null,
            bool? isLongRunning = null,
            LogOptionsEnum loggingOptions = LogOptionsEnum.Default
        ) {
            
            var immediateOperationResult = await base.RunOperation(
                operationName: operationName,
                runSynchronous: IsOperationSynchronous,
                operationAction: operationAction,
                initAction: async () => await InitOperation(),
                preOperationAction: async () => await PreOperation(preCallAction),
                postOperationAction: async (jr) => await PostOperation(jr, postCallAction),
                isLongRunning: isLongRunning,
                loggingOptions: loggingOptions
            );
            return CreateServiceCallResult(immediateOperationResult);
        }

        /// <summary> 
        /// Runs async operation logic in a lambda.  The lambda returns no result.
        /// </summary>
        /// <param name="RequestBody"></param>
        /// <param name="operationAction"></param>
        /// <returns></returns>
        protected async Task<ServiceCallResult> RunServiceCallAs(
            string operationName,
            Func<Task> operationAction,
            Func<Task<bool>> preCallAction = null,
            Func<OperationResult, Task> postCallAction = null,
            bool? isLongRunning = null,
            LogOptionsEnum loggingOptions = LogOptionsEnum.Default
        ) {
            
            var immediateOperationResult = await base.RunOperation(
                operationName: operationName,
                runSynchronous: IsOperationSynchronous,
                operationAction: operationAction,
                initAction: async () => await InitOperation(),
                preOperationAction: async () => await PreOperation(preCallAction),
                postOperationAction: async (jr) => await PostOperation(jr, postCallAction),
                isLongRunning: isLongRunning,
                loggingOptions: loggingOptions
            );
            return CreateServiceCallResult(immediateOperationResult);
        }

        // ---------------------------------------------------------------------
        // InitOperation implementations
        // ---------------------------------------------------------------------

        private async Task<bool> InitOperation() {
            if (!await InitServiceOperation()) {
                Log.Error("Operation init failed", code: BaseMonitorCodes.FailedInit);
                return false;
            }

            return true;
        }

        // ---------------------------------------------------------------------
        // PreOperation implementations
        // ---------------------------------------------------------------------
        private static async Task PreOperation<TRequestBody>(
            TRequestBody request,
            Func<TRequestBody, Task> preCallAction
        ) where TRequestBody : class {

            if (preCallAction != null) {
                await preCallAction(request);
            }
        }

        private static async Task PreOperation(
            Func<Task> preCallAction
        ) {
            if (preCallAction != null) {
                await preCallAction();
            }
        }

        // ---------------------------------------------------------------------
        // PostOperation implementations
        // ---------------------------------------------------------------------

        private static async Task PostOperation<TResponseBody>(
            OperationResult<TResponseBody> result,
            Func<OperationResult<TResponseBody>, Task> postCallAction
        ) where TResponseBody : class {

            if (postCallAction != null) {
                await postCallAction(result);
            }
        }

        private static async Task PostOperation(
            OperationResult result,
            Func<OperationResult, Task> postCallAction
        ) {

            if (postCallAction != null) {
                await postCallAction(result);
            }
        }
    }
}

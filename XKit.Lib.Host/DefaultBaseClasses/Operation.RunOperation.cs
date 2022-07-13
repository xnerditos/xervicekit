using System;
using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Utility.Extensions;

namespace XKit.Lib.Host.DefaultBaseClasses {

    public partial class Operation {

        // ---------------------------------------------------------------------
        // RunOperation<TWorkItem, TResultData> implementations
        // ---------------------------------------------------------------------

        /// <summary>
        /// Runs async operation logic in a lambda.  The lambda must return from a call to End().
        /// </summary>
        /// <param name="request.workItem"></param>
        /// <param name="operationAction"></param>
        /// <returns></returns>
        protected async Task<OperationResult<TResultData>> RunOperation<TWorkItem, TResultData>(
            string operationName,
            TWorkItem workItem,
            bool runSynchronous,
            Func<TWorkItem, Task<OperationResult<TResultData>>> operationAction,
            Func<TWorkItem, bool> workItemValidationAction = null,
            Func<Task<bool>> initAction = null,
            Func<TWorkItem, Task> preOperationAction = null,
            Func<TWorkItem, OperationResult<TResultData>, Task> postOperationAction = null,
            object additionalLogAttributes = null,
            bool? isLongRunning = null,
            LogOptionsEnum loggingOptions = LogOptionsEnum.Default
        ) where TWorkItem : class where TResultData : class {

            if (!BeginOperation(workItem, operationName, additionalLogAttributes, loggingOptions: loggingOptions)) 
            { return ResultCallInvalidServiceUnavailable<TResultData>(); }
            
            var workItemCopy = workItem?.DeepCopy();

            try {
                if (workItemValidationAction != null) {
                    if (!workItemValidationAction(workItemCopy)) {
                        return EndOperation<TResultData>(fromResult: null, LogResultStatusEnum.NoAction_BadRequest);
                    }
                }
                if (initAction != null) {
                    if (!await initAction()) {
                        return EndOperation<TResultData>(fromResult: null, LogResultStatusEnum.RetriableError, operationMessage: "Init failed");
                    }
                }
            } catch (Exception ex) {
                Erratum("Unhandled exception on operation init");
                LogExceptionAsFatality(ex);
                return EndOperation<TResultData>(fromResult: null, LogResultStatusEnum.Fault, ex.Message);
            }

            if (runSynchronous) {
                return await ExecuteMainOperationActions(
                    operationAction, 
                    preOperationAction, 
                    postOperationAction, 
                    workItemCopy
                );
            } else {

                var immediateResult = ResultAndLogCallPending<TResultData>();
                Task task = new Task(
                    async () => await ExecuteMainOperationActions(
                        operationAction, 
                        preOperationAction, 
                        postOperationAction, 
                        workItemCopy
                    ),
                    isLongRunning.GetValueOrDefault(IsLongRunningDefault) ? 
                        TaskCreationOptions.LongRunning :
                        TaskCreationOptions.None
                );
                task.Start();
                task.Forget();
                return immediateResult;
            }
        }

        /// <summary>
        /// Runs async operation logic in a lambda.  The lambda must call simply return the result.
        /// </summary>
        /// <param name="request.workItem"></param>
        /// <param name="operationAction"></param>
        /// <returns></returns>
        protected async Task<OperationResult<TResultData>> RunOperation<TWorkItem, TResultData>(
            string operationName,
            TWorkItem workItem,
            bool runSynchronous,
            Func<TWorkItem, Task<TResultData>> operationAction,
            Func<TWorkItem, bool> workItemValidationAction = null,
            Func<Task<bool>> initAction = null,
            Func<TWorkItem, Task> preOperationAction = null,
            Func<TWorkItem, OperationResult<TResultData>, Task> postOperationAction = null,
            object additionalLogAttributes = null,
            bool? isLongRunning = null,
            LogOptionsEnum loggingOptions = LogOptionsEnum.Default
        ) where TWorkItem : class where TResultData : class {
            return await RunOperation<TWorkItem, TResultData>(
                operationName,
                workItem,
                runSynchronous,
                async(w) => Result(operationStatus: null, await operationAction(w)),
                workItemValidationAction,
                initAction,
                preOperationAction,
                postOperationAction,
                additionalLogAttributes,
                isLongRunning,
                loggingOptions
            );
        }

        private async Task<OperationResult<TResultData>> ExecuteMainOperationActions<TWorkItem, TResultData>(
            Func<TWorkItem, Task<OperationResult<TResultData>>> operationAction, 
            Func<TWorkItem, Task> preOperationAction, 
            Func<TWorkItem, OperationResult<TResultData>, Task> postOperationAction, 
            TWorkItem workItem
        ) where TWorkItem : class where TResultData : class {
            
            OperationResult<TResultData> opResult = null;
            Exception operationException = null;
            try {

                if (preOperationAction != null) {
                    await preOperationAction(workItem);
                }
                opResult = await operationAction(workItem);

            } catch (Exception ex) {
                LogExceptionAsFatality(ex);
                operationException = ex;
            } finally {

                if (postOperationAction != null) {
                    try { 
                        await postOperationAction(workItem, opResult); 
                    } 
                    catch (Exception postOperationException) {
                        // eat exception after logging
                        LogExceptionAsErratum(postOperationException, "Unhandled exception on post operation");
                    }
                }
            }
            
            return EndOperation(
                fromResult: opResult, 
                operationStatus: operationException != null ? LogResultStatusEnum.Fault : null,
                logMessage: operationException?.Message
            );
        }

        // ---------------------------------------------------------------------
        // RunOperation<TResultData> implementations
        // ---------------------------------------------------------------------

        /// <summary>
        /// Runs async operation logic in a lambda.  The lambda must return from a call to End().
        /// </summary>
        /// <param name="request.workItem"></param>
        /// <param name="operationAction"></param>
        /// <returns></returns>
        protected async Task<OperationResult<TResultData>> RunOperation<TResultData>(
            string operationName,
            bool runSynchronous,
            Func<Task<OperationResult<TResultData>>> operationAction,
            Func<Task<bool>> initAction = null,
            Func<Task> preOperationAction = null,
            Func<OperationResult<TResultData>, Task> postOperationAction = null,
            object additionalLogAttributes = null,
            bool? isLongRunning = null,
            LogOptionsEnum loggingOptions = LogOptionsEnum.Default
        ) where TResultData : class {

            if (!BeginOperation<object>(null, operationName, additionalLogAttributes, loggingOptions: loggingOptions)) 
            { return ResultCallInvalidServiceUnavailable<TResultData>(); }
            
            try {
                if (initAction != null) {
                    if (!await initAction()) {
                        return EndOperation<TResultData>(fromResult: null, LogResultStatusEnum.RetriableError, operationMessage: "Init failed");
                    }
                }
            } catch (Exception ex) {
                Erratum("Unhandled exception on operation init");
                LogExceptionAsFatality(ex);
                return EndOperation<TResultData>(fromResult: null, LogResultStatusEnum.Fault, ex.Message);
            }

            if (runSynchronous) {
                return await ExecuteMainOperationActions(
                    operationAction, 
                    preOperationAction, 
                    postOperationAction
                );
            } else {

                var immediateResult = ResultAndLogCallPending<TResultData>();
                Task task = new Task(
                    async () => await ExecuteMainOperationActions(
                        operationAction, 
                        preOperationAction, 
                        postOperationAction
                    ),
                    isLongRunning.GetValueOrDefault(IsLongRunningDefault) ? 
                        TaskCreationOptions.LongRunning :
                        TaskCreationOptions.None
                );
                task.Start();
                task.Forget();
                return immediateResult;
            }
        }

        /// <summary>
        /// Runs async operation logic in a lambda.  The lambda must call simply return the result.
        /// </summary>
        /// <param name="request.workItem"></param>
        /// <param name="operationAction"></param>
        /// <returns></returns>
        protected async Task<OperationResult<TResultData>> RunOperation<TResultData>(
            string operationName,
            bool runSynchronous,
            Func<Task<TResultData>> operationAction,
            Func<Task<bool>> initAction = null,
            Func<Task> preOperationAction = null,
            Func<OperationResult<TResultData>, Task> postOperationAction = null,
            object additionalLogAttributes = null,
            bool? isLongRunning = null,
            LogOptionsEnum loggingOptions = LogOptionsEnum.Default
        ) where TResultData : class {
            return await RunOperation<TResultData>(
                operationName,
                runSynchronous,
                async() => Result(operationStatus: null, await operationAction()),
                initAction,
                preOperationAction,
                postOperationAction,
                additionalLogAttributes,
                isLongRunning,
                loggingOptions
            );
        }

        private async Task<OperationResult<TResultData>> ExecuteMainOperationActions<TResultData>(
            Func<Task<OperationResult<TResultData>>> operationAction, 
            Func<Task> preOperationAction, 
            Func<OperationResult<TResultData>, Task> postOperationAction
        ) where TResultData : class {
            
            OperationResult<TResultData> opResult = null;
            Exception operationException = null;
            try {

                if (preOperationAction != null) {
                    await preOperationAction();
                }
                opResult = await operationAction();

            } catch (Exception ex) {
                LogExceptionAsFatality(ex);
                operationException = ex;
            } finally {

                if (postOperationAction != null) {
                    try { 
                        await postOperationAction(opResult); 
                    } 
                    catch (Exception postOperationException) {
                        // eat exception after logging
                        LogExceptionAsErratum(postOperationException, "Unhandled exception on post operation");
                    }
                }
            }
            
            return EndOperation(
                fromResult: opResult, 
                operationStatus: operationException != null ? LogResultStatusEnum.Fault : null,
                logMessage: operationException?.Message
            );
        }

        // ---------------------------------------------------------------------
        // RunOperation<TWorkItem> implementations
        // ---------------------------------------------------------------------

        /// <summary> 
        /// Runs async operation logic in a lambda.  The lambda returns no result.
        /// </summary>
        /// <param name="workItem"></param>
        /// <param name="operationAction"></param>
        /// <returns></returns>
        protected async Task<OperationResult> RunOperation<TWorkItem>(
            string operationName,
            TWorkItem workItem,
            bool runSynchronous,
            Func<TWorkItem, Task<OperationResult>> operationAction,
            Func<TWorkItem, bool> workItemValidationAction = null,
            Func<Task<bool>> initAction = null,
            Func<TWorkItem, Task> preOperationAction = null,
            Func<TWorkItem, OperationResult, Task> postOperationAction = null,
            object additionalLogAttributes = null,
            bool? isLongRunning = null,
            LogOptionsEnum loggingOptions = LogOptionsEnum.Default
        ) where TWorkItem : class {

            if (!BeginOperation(workItem, operationName, additionalLogAttributes, loggingOptions: loggingOptions)) 
            { return ResultCallInvalidServiceUnavailable(); }
            
            var workItemCopy = workItem?.DeepCopy();

            try {
                if (workItemValidationAction != null) {
                    if (!workItemValidationAction(workItemCopy)) {
                        return EndOperation(fromResult: null, LogResultStatusEnum.NoAction_BadRequest);
                    }
                }
                if (initAction != null) {
                    if (!await initAction()) {
                        return EndOperation(fromResult: null, LogResultStatusEnum.RetriableError, operationMessage: "Init failed");
                    }
                }
            } catch (Exception ex) {
                Erratum("Unhandled exception on operation init");
                LogExceptionAsFatality(ex);
                return EndOperation(fromResult: null, LogResultStatusEnum.Fault, ex.Message);
            }

            if (runSynchronous) {
                return await ExecuteMainOperationActions(
                    operationAction, 
                    preOperationAction, 
                    postOperationAction, 
                    workItemCopy
                );
            } else {

                var immediateResult = ResultAndLogCallPending();
                Task task = new Task(
                    async () => await ExecuteMainOperationActions(
                        operationAction, 
                        preOperationAction, 
                        postOperationAction, 
                        workItemCopy
                    ),
                    isLongRunning.GetValueOrDefault(IsLongRunningDefault) ? 
                        TaskCreationOptions.LongRunning :
                        TaskCreationOptions.None
                );
                task.Start();
                task.Forget();
                return immediateResult;
            }
        }

        /// <summary> 
        /// Runs async operation logic in a lambda.  The lambda returns no result.
        /// </summary>
        /// <param name="workItem"></param>
        /// <param name="operationAction"></param>
        /// <returns></returns>
        protected async Task<OperationResult> RunOperation<TWorkItem>(
            string operationName,
            TWorkItem workItem,
            bool runSynchronous,
            Func<TWorkItem, Task> operationAction,
            Func<TWorkItem, bool> workItemValidationAction = null,
            Func<Task<bool>> initAction = null,
            Func<TWorkItem, Task> preOperationAction = null,
            Func<TWorkItem, OperationResult, Task> postOperationAction = null,
            object additionalLogAttributes = null,
            bool? isLongRunning = null,
            LogOptionsEnum loggingOptions = LogOptionsEnum.Default
        ) where TWorkItem : class {
            return await RunOperation(
                operationName,
                workItem,
                runSynchronous,
                (Func<TWorkItem, Task<OperationResult>>) (async(w) => {
                    await operationAction(w);
                    return Result(operationStatus: null);
                }),
                workItemValidationAction,
                initAction,
                preOperationAction,
                postOperationAction,
                additionalLogAttributes,
                isLongRunning,
                loggingOptions
            );
        }

        private async Task<OperationResult> ExecuteMainOperationActions<TWorkItem>(
            Func<TWorkItem, Task<OperationResult>> operationAction, 
            Func<TWorkItem, Task> preOperationAction, 
            Func<TWorkItem, OperationResult, Task> postOperationAction, 
            TWorkItem workItem
        ) where TWorkItem : class {
            
            OperationResult opResult = null;
            Exception operationException = null;
            try {

                if (preOperationAction != null) {
                    await preOperationAction(workItem);
                }
                opResult = await operationAction(workItem);

            } catch (Exception ex) {
                LogExceptionAsFatality(ex);
                operationException = ex;
            } finally {

                if (postOperationAction != null) {
                    try { 
                        await postOperationAction(workItem, opResult); 
                    } 
                    catch (Exception postOperationException) {
                        // eat exception after logging
                        LogExceptionAsErratum(postOperationException, "Unhandled exception on post operation");
                    }
                }
            }
            
            return EndOperation(
                fromResult: opResult, 
                operationStatus: operationException != null ? LogResultStatusEnum.Fault : null,
                logMessage: operationException?.Message
            );
        }

        // ---------------------------------------------------------------------
        // RunOperation<> implementations
        // ---------------------------------------------------------------------

        /// <summary> 
        /// Runs async operation logic in a lambda.  The lambda returns no result.
        /// </summary>
        /// <param name="operationAction"></param>
        /// <returns></returns>
        protected async Task<OperationResult> RunOperation(
            string operationName,
            bool runSynchronous,
            Func<Task<OperationResult>> operationAction,
            Func<Task<bool>> initAction = null,
            Func<Task> preOperationAction = null,
            Func<OperationResult, Task> postOperationAction = null,
            object additionalLogAttributes = null,
            bool? isLongRunning = null,
            LogOptionsEnum loggingOptions = LogOptionsEnum.Default
        ) {

            if (!BeginOperation<object>(null, operationName, additionalLogAttributes, loggingOptions: loggingOptions)) 
            { return ResultCallInvalidServiceUnavailable(); }
            
            try {
                if (initAction != null) {
                    if (!await initAction()) {
                        return EndOperation(fromResult: null, LogResultStatusEnum.RetriableError, operationMessage: "Init failed");
                    }
                }
            } catch (Exception ex) {
                Erratum("Unhandled exception on operation init");
                LogExceptionAsFatality(ex);
                return EndOperation(fromResult: null, LogResultStatusEnum.Fault, ex.Message);
            }

            if (runSynchronous) {
                return await ExecuteMainOperationActions(
                    operationAction, 
                    preOperationAction, 
                    postOperationAction
                );
            } else {

                var immediateResult = ResultAndLogCallPending();
                Task task = new Task(
                    async () => await ExecuteMainOperationActions(
                        operationAction, 
                        preOperationAction, 
                        postOperationAction
                    ),
                    isLongRunning.GetValueOrDefault(IsLongRunningDefault) ? 
                        TaskCreationOptions.LongRunning :
                        TaskCreationOptions.None
                );
                task.Start();
                task.Forget();
                return immediateResult;
            }
        }

        /// <summary> 
        /// Runs async operation logic in a lambda.  The lambda returns no result.
        /// </summary>
        /// <param name="workItem"></param>
        /// <param name="operationAction"></param>
        /// <returns></returns>
        protected async Task<OperationResult> RunOperation(
            string operationName,
            bool runSynchronous,
            Func<Task> operationAction,
            Func<Task<bool>> initAction = null,
            Func<Task> preOperationAction = null,
            Func<OperationResult, Task> postOperationAction = null,
            object additionalLogAttributes = null,
            bool? isLongRunning = null,
            LogOptionsEnum loggingOptions = LogOptionsEnum.Default
        ) {
            return await RunOperation(
                operationName,
                runSynchronous,
                (Func<Task<OperationResult>>) (async() => {
                    await operationAction();
                    return Result(operationStatus: null);
                }),
                initAction,
                preOperationAction,
                postOperationAction,
                additionalLogAttributes,
                isLongRunning,
                loggingOptions
            );
        }

        private async Task<OperationResult> ExecuteMainOperationActions(
            Func<Task<OperationResult>> operationAction, 
            Func<Task> preOperationAction, 
            Func<OperationResult, Task> postOperationAction
        ) {
            OperationResult opResult = null;
            Exception operationException = null;
            try {

                if (preOperationAction != null) {
                    await preOperationAction();
                }
                opResult = await operationAction();

            } catch (Exception ex) {
                LogExceptionAsFatality(ex);
                operationException = ex;
            } finally {

                if (postOperationAction != null) {
                    try { 
                        await postOperationAction(opResult); 
                    } 
                    catch (Exception postOperationException) {
                        // eat exception after logging
                        LogExceptionAsErratum(postOperationException, "Unhandled exception on post operation");
                    }
                }
            }
            
            return EndOperation(
                fromResult: opResult, 
                operationStatus: operationException != null ? LogResultStatusEnum.Fault : null,
                logMessage: operationException?.Message
            );
        }
    }
}

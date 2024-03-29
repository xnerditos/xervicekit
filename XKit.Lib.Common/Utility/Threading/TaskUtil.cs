using System;
using System.Threading;
using System.Threading.Tasks;

namespace XKit.Lib.Common.Utility.Threading {

    /// <summary>
    /// Helper class to run async methods within a sync process.
    /// </summary>
    public static class TaskUtil {
        private static readonly TaskFactory taskFactory =
            new TaskFactory(
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskContinuationOptions.None,
                TaskScheduler.Default
            );
 
        /// <summary>
        /// Executes an async Task method which has a void return value synchronously
        /// USAGE: AsyncUtil.RunSync(() => AsyncMethod());
        /// </summary>
        /// <param name="task">Task method to execute</param>
        public static void RunAsyncAsSync(Func<Task> runner, bool longRunning = false)
            => taskFactory
                .StartNew(runner, longRunning ? TaskCreationOptions.LongRunning : TaskCreationOptions.None)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
 
        /// <summary>
        /// Executes an async Task<T> method which has a T return type synchronously
        /// USAGE: T result = AsyncUtil.RunSync(() => AsyncMethod<T>());
        /// </summary>
        /// <typeparam name="TResult">Return Type</typeparam>
        /// <param name="task">Task<T> method to execute</param>
        /// <returns></returns>
        public static TResult RunAsyncAsSync<TResult>(Func<Task<TResult>> runner, bool longRunning = false)
            => taskFactory
                .StartNew(runner, longRunning ? TaskCreationOptions.LongRunning : TaskCreationOptions.None)
                .Unwrap()
                .GetAwaiter()
                .GetResult();

        public static Task<TResult> WrapInTask<TResult>(Func<TResult> action) {
            try {
                var result = action.Invoke();
                return Task.FromResult(result);
            } catch (Exception ex) {
                return Task.FromException<TResult>(ex);
            }
        }

        public static Task WrapInTask(Action action) {
            try {
                action.Invoke();
                return Task.CompletedTask;
            } catch (Exception ex) {
                return Task.FromException(ex);
            }
        }

        public static Task<TResult> RunSyncAsAsync<TResult>(Func<TResult> action, bool longRunning = false) 
            => new(
                action,
                longRunning ? TaskCreationOptions.LongRunning : TaskCreationOptions.None
            );

        public static Task RunSyncAsAsync(Action action, bool longRunning = false) 
            => new(
                action,
                longRunning ? TaskCreationOptions.LongRunning : TaskCreationOptions.None
            );
    }
}

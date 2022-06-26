using System;
using System.Threading.Tasks;

namespace XKit.Lib.Common.Utility.Extensions {
    public static class TaskExtensions {

        public static void Forget(this Task task, Action<Exception> exceptionHandler = null) {
            
            // Only care about tasks that may fault (not completed) or are faulted,
            // so fast-path for SuccessfullyCompleted and Canceled tasks.
            // Allocate the async/await state machine only when needed for performance reason.
            // More info about the state machine: https://blogs.msdn.microsoft.com/seteplia/2017/11/30/dissecting-the-async-methods-in-c/
            if (!task.IsCompleted || task.IsFaulted) {
                if (exceptionHandler == null) {
                    task.ContinueWith(
                        DefaultErrorContinuation,
                        TaskContinuationOptions.ExecuteSynchronously |
                        TaskContinuationOptions.OnlyOnFaulted
                    );
                }
                else {
                    task.ContinueWith(
                        t => exceptionHandler(t.Exception.GetBaseException()),
                        TaskContinuationOptions.ExecuteSynchronously |
                        TaskContinuationOptions.OnlyOnFaulted);
                }
            }
        }

        private static readonly Action<Task> DefaultErrorContinuation = t => {
            try { t.Wait(); }
            catch {}
        };
    }
}

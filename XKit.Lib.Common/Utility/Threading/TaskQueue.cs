using System;
using System.Threading.Tasks;

namespace XKit.Lib.Common.Utility.Threading {

    // Attribution:  https://github.com/Gentlee/SerialQueue

    /// <summary>
    /// A lightweight mechanism for execution of tasks in order
    /// </summary>
    public class TaskQueue {
        private readonly object locker = new object();
        private readonly WeakReference<Task> lastTaskReference = new WeakReference<Task>(null);

        public Task Enqueue(Action action) {
            return Enqueue<bool>(() => {
                action();
                return true;
            });
        }

        public Task<T> Enqueue<T>(Func<T> function) {

            lock(locker) {
                Task lastTask;
                Task<T> resultTask;

                if (lastTaskReference.TryGetTarget(out lastTask)) {
                    resultTask = lastTask.ContinueWith(_ => function(), TaskContinuationOptions.ExecuteSynchronously);
                } else {
                    resultTask = Task.Run(function);
                }

                lastTaskReference.SetTarget(resultTask);

                return resultTask;
            }
        }

        public Task Enqueue(Func<Task> asyncAction) {
            lock(locker) {
                Task lastTask;
                Task resultTask;

                if (lastTaskReference.TryGetTarget(out lastTask)) {
                    resultTask = lastTask.ContinueWith(_ => asyncAction(), TaskContinuationOptions.ExecuteSynchronously).Unwrap();
                } else {
                    resultTask = Task.Run(asyncAction);
                }

                lastTaskReference.SetTarget(resultTask);

                return resultTask;
            }
        }

        public Task<T> Enqueue<T>(Func<Task<T>> asyncFunction) {
            lock(locker) {
                Task lastTask;
                Task<T> resultTask;

                if (lastTaskReference.TryGetTarget(out lastTask)) {
                    resultTask = lastTask.ContinueWith(_ => asyncFunction(), TaskContinuationOptions.ExecuteSynchronously).Unwrap();
                } else {
                    resultTask = Task.Run(asyncFunction);
                }

                lastTaskReference.SetTarget(resultTask);

                return resultTask;
            }
        }

        public bool HasTasks {
            get {
                if (!lastTaskReference.TryGetTarget(out var lastTask)) {
                    return false;
                }
                return !lastTask.IsCompleted;
            }
        }

        public Task LastTask {
            get {
                Task lastTask = null;
                lastTaskReference.TryGetTarget(out lastTask);
                return lastTask;
            }
        }
    }
}
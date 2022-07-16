using System;
using XKit.Lib.Common.Log;

namespace XKit.Lib.Common.Host
{
    public interface IGenericManagedService : IManagedService {

        /// <summary>
        /// Adds a daemon to this generic service
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        void AddDaemon<TMessage>(
            IServiceDaemon<TMessage> daemon
        ) where TMessage : class;

        IGenericTimerDaemon AddGenericTimerDaemon<TDaemonOperation>(
            uint? timerDelayMilliseconds = null,
            bool timerEnabled = true,
            Action<IGenericTimerDaemon> onEnvironmentChangeHandler = null,
            string name = null
        ) where TDaemonOperation : IGenericTimerDaemonOperation;
    }
    
    public interface IGenericManagedService<TOperationConcrete> : IGenericManagedService {}
}

using System;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Log;
using XKit.Lib.Host.DefaultBaseClasses;

namespace XKit.Lib.Host.Services {

    public class GenericTimerDaemon<TDaemonTimerOperation> : ServiceDaemon<TDaemonTimerOperation, object>, IGenericTimerDaemon 
        where TDaemonTimerOperation : IGenericTimerDaemonOperation {
        
        public const int DefaultTimerDelayMilliseconds = 1000 * 60;     // 1 minute

        private readonly string name;
        private uint? nextEventDelay = default;
        protected override string Name => name;
        private readonly Action<IGenericTimerDaemon> onEnvironmentChangeHandler;

        public GenericTimerDaemon(
            ILogSessionFactory logSessionFactory, 
            uint? timerDelayMilliseconds = null,
            bool timerEnabled = true,
            Action<IGenericTimerDaemon> onEnvironmentChangeHandler = null,
            string name = null
        ) : base(logSessionFactory) {
            this.name = name ?? $"GenericDaemonFor_{typeof(TDaemonTimerOperation).Name}";
            nextEventDelay = timerDelayMilliseconds.GetValueOrDefault(DefaultTimerDelayMilliseconds);
            this.onEnvironmentChangeHandler = onEnvironmentChangeHandler;
            MaxConcurrentMessages = 1;
            EnableTimerEvent = timerEnabled;
        }

        public void SetTimerInterval(uint? milliseconds) {
            nextEventDelay = milliseconds;
        }

        public void SetTimerEnabled(bool enabled) {
            EnableTimerEvent = enabled;
        }

        protected override void OnEnvironmentChange() {
            onEnvironmentChangeHandler?.Invoke(this);
        }

        protected override uint? OnDetermineTimerEventPeriod() => nextEventDelay;

        void IGenericTimerDaemon.SetTimerEnabled(bool enabled) => SetTimerEnabled(enabled);

        void IGenericTimerDaemon.SetTimerInterval(uint? milliseconds) => SetTimerInterval(milliseconds);
    }
}

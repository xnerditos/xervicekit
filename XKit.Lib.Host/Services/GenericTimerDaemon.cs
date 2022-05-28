using System;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Log;
using XKit.Lib.Host.DefaultBaseClasses;

namespace XKit.Lib.Host.Services {

    public class GenericTimerDaemon<TDaemonOperation> : ServiceDaemon<TDaemonOperation>, IGenericTimerDaemon 
        where TDaemonOperation : IServiceDaemonOperation<object> {
        
        public const int DefaultTimerDelayMilliseconds = 1000 * 60;     // 1 minute

        private readonly string name;
        private Action<IGenericTimerDaemon> onEnvironmentChangeHandler;
        protected override string Name => name;

        public GenericTimerDaemon(
            ILogSessionFactory logSessionFactory, 
            int? timerDelayMilliseconds = null,
            bool timerEnabled = true,
            string name = null,
            Action<IGenericTimerDaemon> onEnvironmentChangeHandler = null
        ) : base(logSessionFactory) {
            this.name = name ?? $"GenericDaemonFor_{typeof(TDaemonOperation).Name}";
            this.onEnvironmentChangeHandler = onEnvironmentChangeHandler;
            this.WakeDelayMillisecondsWhenNoMessagesWaiting = timerDelayMilliseconds.GetValueOrDefault(DefaultTimerDelayMilliseconds);
            this.MaxConcurrentMessages = 1;
            this.AutoPulseActive = true;
            this.EnableEnqueueEvent = timerEnabled;
        }

        public void SetTimerDelay(int? milliseconds) {
            if (!milliseconds.HasValue) { return; }
            this.WakeDelayMillisecondsWhenNoMessagesWaiting = milliseconds.Value;
        }

        public void SetNextEventReadyDelay(int? milliseconds) {
            if (!milliseconds.HasValue) { return; }
            this.WakeDelayMillisecondsWhenMessagesWaiting = milliseconds.Value;
        }

        public void SetTimerEnabled(bool? enabled) {
            if (!enabled.HasValue) { return; }
            this.EnableEnqueueEvent = enabled.Value;
        }

        protected override void OnEnvironmentChange() {
            if (this.onEnvironmentChangeHandler != null) {
                onEnvironmentChangeHandler.Invoke(this);
                Pulse();
            }
        }

        protected override void OnEnqueueEvent() {
            PostMessage(new object(), false);
        }

        void IGenericTimerDaemon.ManuallyPostNewTimerEvent() {
            PostMessage(new object(), true);
        }

        protected override TimeSpan? OnDetermineEnqueueEventPeriod() 
            => TimeSpan.FromMilliseconds(this.WakeDelayMillisecondsWhenNoMessagesWaiting);

        void IGenericTimerDaemon.SetTimerDelay(int? milliseconds) => SetTimerDelay(milliseconds);

        void IGenericTimerDaemon.SetTimerEnabled(bool? enabled) => SetTimerEnabled(enabled);

        void IGenericTimerDaemon.SetNextEventReadyDelay(int? milliseconds) => SetNextEventReadyDelay(milliseconds);
    }
}

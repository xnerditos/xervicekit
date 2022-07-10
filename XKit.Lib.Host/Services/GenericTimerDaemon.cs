using System;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Log;
using XKit.Lib.Host.DefaultBaseClasses;

namespace XKit.Lib.Host.Services {

    public class GenericTimerDaemon<TDaemonTimerOperation> : ServiceDaemon<NoOpDaemonMessageOperation, object, TDaemonTimerOperation>, IGenericTimerDaemon 
        where TDaemonTimerOperation : IServiceDaemonOperation {
        
        public const int DefaultTimerDelayMilliseconds = 1000 * 60;     // 1 minute

        private readonly string name;
        private readonly Action<IGenericTimerDaemon> onEnvironmentChangeHandler;
        private readonly Func<bool> onDetermineCanRunOperation;
        private readonly Func<IGenericTimerDaemon, bool> onOperationFinished;
        private uint? nextEventDelay = default;
        protected override string Name => name;

        public GenericTimerDaemon(
            ILogSessionFactory logSessionFactory, 
            Func<bool> onDetermineCanRunOperation = null,
            Func<IGenericTimerDaemon, bool> onOperationFinished = null,
            uint? timerDelayMilliseconds = null,
            bool timerEnabled = true,
            string name = null,
            Action<IGenericTimerDaemon> onEnvironmentChangeHandler = null
        ) : base(logSessionFactory) {
            this.name = name ?? $"GenericDaemonFor_{typeof(TDaemonTimerOperation).Name}";
            this.onEnvironmentChangeHandler = onEnvironmentChangeHandler;
            this.onDetermineCanRunOperation = onDetermineCanRunOperation;
            this.onOperationFinished = onOperationFinished;
            DefaultTimerPeriodMilliseconds = timerDelayMilliseconds.GetValueOrDefault(DefaultTimerDelayMilliseconds);
            MaxConcurrentMessages = 1;
            EnableTimerEvent = timerEnabled;
        }

        public void SetDefaultTimerDelay(uint milliseconds) {
            this.DefaultTimerPeriodMilliseconds = milliseconds;
        }

        public void SetNextEventReadyDelay(uint? milliseconds) {
            nextEventDelay = milliseconds;
        }

        public void SetTimerEnabled(bool enabled) {
            EnableTimerEvent = enabled;
        }

        protected override void OnEnvironmentChange() {
            if (this.onEnvironmentChangeHandler != null) {
                onEnvironmentChangeHandler.Invoke(this);
            }
        }

        protected override void OnTimerEvent() {
            try {
                if (onDetermineCanRunOperation?.Invoke() ?? true) {
                    EnableTimerEvent = false;
                    PostMessage(new());
                }
            } catch(Exception exception) {
                var attributes = new { exception };
                Log.Erratum("Exception thrown while posting messages", attributes);
                Log.Fatality(exception.Message, attributes);
            } 
        }

        protected override void OnEndProcessingMessages() {
            try {
                onOperationFinished?.Invoke(this);
                EnableTimerEvent = true;
            } catch(Exception exception) {
                var attributes = new { exception };
                Log.Erratum("Exception thrown while posting messages", attributes);
            } 
        }
        protected override uint? OnDetermineTimerEventPeriod() => this.nextEventDelay;

        void IGenericTimerDaemon.SetTimerDelay(uint milliseconds) => SetDefaultTimerDelay(milliseconds);

        void IGenericTimerDaemon.SetTimerEnabled(bool enabled) => SetTimerEnabled(enabled);

        void IGenericTimerDaemon.SetNextEventReadyDelay(uint? milliseconds) => SetNextEventReadyDelay(milliseconds);
    }
}

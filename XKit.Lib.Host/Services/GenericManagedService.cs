using System;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;
using XKit.Lib.Host.DefaultBaseClasses;

namespace XKit.Lib.Host.Services {

    public class GenericManagedService<TOperation>
        : ManagedService<TOperation>,
        IGenericManagedService<TOperation>
        where TOperation : IServiceOperation {

            private readonly Descriptor descriptor;

            public GenericManagedService(
                Descriptor descriptor,
                ILocalEnvironment localFabric
            ) : base(
                localFabric
            ) {
                this.descriptor = descriptor;
            }

            // =====================================================================
            // base class overrides
            // =====================================================================

            protected override IReadOnlyDescriptor Descriptor => descriptor;

            // =====================================================================
            // IGenericManagedService
            // =====================================================================

            void IGenericManagedService.AddDaemon<TMessage>(IServiceDaemon<TMessage> daemon) => this.AddDaemon(daemon);

            IGenericTimerDaemon IGenericManagedService.AddGenericTimerDaemon<TDaemonOperation>(
                ILogSessionFactory logSessionFactory, 
                Func<bool> onDetermineCanRunOperation,
                Func<IGenericTimerDaemon, bool> onOperationFinished,
                uint? timerDelayMilliseconds,
                bool timerEnabled,
                string name,
                Action<IGenericTimerDaemon> onEnvironmentChangeHandler
            ) {
                var daemon = new GenericTimerDaemon<TDaemonOperation>(
                    logSessionFactory,
                    onDetermineCanRunOperation,
                    onOperationFinished,
                    timerDelayMilliseconds,
                    timerEnabled,
                    name,
                    onEnvironmentChangeHandler: onEnvironmentChangeHandler
                );
                this.AddDaemon(daemon);
                return daemon;
            }
        }
}

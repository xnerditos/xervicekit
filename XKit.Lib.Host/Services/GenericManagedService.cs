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
                int? timerDelayMilliseconds, 
                string daemonName, 
                Action<IGenericTimerDaemon> onEnvironmentChangeHandler
            ) {
                var daemon = new GenericTimerDaemon<TDaemonOperation>(
                    logSessionFactory,
                    timerDelayMilliseconds,
                    true,
                    daemonName,
                    onEnvironmentChangeHandler: onEnvironmentChangeHandler
                );
                this.AddDaemon(daemon);
                return daemon;
            }
        }
}

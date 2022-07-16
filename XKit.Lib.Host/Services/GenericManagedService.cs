using System;
using XKit.Lib.Common.Host;
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
                IXKitHostEnvironment hostEnv
            ) : base(
                hostEnv
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
                uint? timerDelayMilliseconds,
                bool timerEnabled,
                Action<IGenericTimerDaemon> onEnvironmentChangeHandler,
                string name
            ) {
                var daemon = new GenericTimerDaemon<TDaemonOperation>(
                    HostEnvironment.LogSessionFactory,
                    timerDelayMilliseconds,
                    timerEnabled,
                    onEnvironmentChangeHandler,
                    name
                );
                this.AddDaemon(daemon);
                return daemon;
            }
        }
}

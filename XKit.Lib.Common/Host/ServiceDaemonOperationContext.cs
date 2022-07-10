using System;

namespace XKit.Lib.Common.Host {

    /// <summary>
    /// Provides the context of a service operation, with information
    /// particular to the lifecycle of the operation.
    /// </summary>
    public class ServiceDaemonOperationContext : OperationContext {

        public ServiceDaemonOperationContext(
            IServiceDaemon daemon,
            IServiceBase service,
            IXKitHostEnvironment hostEnv,
            string correlationId = null
        ) : base(
            hostEnv,
            correlationId
        ) {
            this.Service = service ?? throw new ArgumentNullException(nameof(service));
            this.Daemon = daemon ?? throw new ArgumentNullException(nameof(daemon));
        }

        // =====================================================================
        // public
        // =====================================================================

        public IServiceBase Service { get; }
        public IServiceDaemon Daemon { get; }
    }
}

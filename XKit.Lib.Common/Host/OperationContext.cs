using System;
using XKit.Lib.Common.Fabric;

namespace XKit.Lib.Common.Host {

    /// <summary>
    /// Provides the context of a service operation, with information
    /// particular to the lifecycle of the operation.
    /// </summary>
    public class OperationContext {

        public OperationContext(
            IXkitHostEnvironment hostEnv,
            string correlationId
        ) {
            this.HostEnv = hostEnv ?? throw new ArgumentNullException(nameof(hostEnv));
            this.CorrelationId = correlationId ?? Common.Utility.Identifiers.GenerateIdentifier();
            this.OperationId = Guid.NewGuid();
        }

        // =====================================================================
        // public
        // =====================================================================

        public Guid OperationId { get; }
        public string CorrelationId { get; }
        public IXkitHostEnvironment HostEnv { get; }
        public IFabricConnector Connector => HostEnv.Connector;
    }
}

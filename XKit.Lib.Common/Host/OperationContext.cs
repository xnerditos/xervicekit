using System;
using System.Collections.Generic;
using XKit.Lib.Common.Fabric;

namespace XKit.Lib.Common.Host {

    /// <summary>
    /// Provides the context of a service operation, with information
    /// particular to the lifecycle of the operation.
    /// </summary>
    public class OperationContext {

        public OperationContext(
            ILocalEnvironment localEnvironment,
            string correlationId
        ) {
            this.LocalEnvironment = localEnvironment ?? throw new ArgumentNullException(nameof(localEnvironment));
            this.CorrelationId = correlationId ?? Common.Utility.Identifiers.GenerateIdentifier();
            this.OperationId = Guid.NewGuid();
        }

        // =====================================================================
        // public
        // =====================================================================

        public Guid OperationId { get; }
        public string CorrelationId { get; }
        public ILocalEnvironment LocalEnvironment { get; }
        public IDependencyConnector DependencyConnector => LocalEnvironment.DependencyConnector;
    }
}

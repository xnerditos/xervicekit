using System;
using XKit.Lib.Common.Fabric;

namespace XKit.Lib.Common.Host {

    /// <summary>
    /// Provides the context of a service operation, with information
    /// particular to the lifecycle of the operation.
    /// </summary>
    public class ServiceOperationContext : OperationContext {

        public ServiceOperationContext(
            IServiceOperationOwner service,
            ILocalEnvironment localEnv,
            ServiceCallTypeParameters callTypeParameters,
            string requestorInstanceId,
            string requestorFabricId,
            string correlationId
        ) : base(
            localEnv,
            correlationId
        ) {
            //this.Service = (service as IServiceOperationOwner) ?? throw new ArgumentNullException(nameof(service));
            this.Service = service ?? throw new ArgumentNullException(nameof(service));
            this.CallParameters = callTypeParameters;
            this.RequestorInstanceId = requestorInstanceId;
            this.RequestorFabricId = requestorFabricId;
        }

        // =====================================================================
        // public
        // =====================================================================

        public IServiceOperationOwner Service { get; }
        public ServiceCallTypeParameters CallParameters { get; }
        public string RequestorInstanceId { get; }
        public string RequestorFabricId { get; }
    }
}

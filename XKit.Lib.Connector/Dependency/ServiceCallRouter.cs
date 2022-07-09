using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;

namespace XKit.Lib.Connector.Dependency {

    internal class ServiceCallRouter : IServiceCallRouter {

        private readonly IReadOnlyServiceRegistration TargetService;
        private readonly List<IInstanceClient> InstanceClients = new();
        private readonly Random Random = new();
        private DateTime validUntilTime;

        public ServiceCallRouter(
            IReadOnlyServiceRegistration targetService,
            DateTime validUntilTime,
            IEnumerable<IInstanceClient> instanceClients
        ) {
            this.TargetService = targetService;
            this.validUntilTime = validUntilTime;
            this.InstanceClients.AddRange(instanceClients);
        }

        // =====================================================================
        // IServiceCallRouter
        // =====================================================================

        string IServiceCallRouter.ServiceRegistrationKey => TargetService.RegistrationKey;
        IReadOnlyDescriptor IServiceCallRouter.ServiceDescriptor => TargetService.Descriptor;
        bool IServiceCallRouter.IsValid => DateTime.UtcNow <= this.validUntilTime;

        void IServiceCallRouter.Invalidate() =>
            this.validUntilTime = DateTime.MinValue;

        async Task<IReadOnlyList<ServiceCallResult>> IServiceCallRouter.ExecuteCall(
            ServiceCallRequest request,
            ILogSession log,
            ServiceCallPolicy policy,
            string targetHostId
        ) {

            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            if (string.IsNullOrEmpty(request.OperationName)) {
                throw new ArgumentException("Operation name is null or empty", nameof(request));
            }

            var useRequest = request.Clone();
            useRequest.CallTypeParameters ??= ServiceCallTypeParameters.SyncResult();

            ServiceCallPatternEnum useCallPattern = 
                policy?.CallPattern ??
                TargetService.CallPolicy?.CallPattern ?? 
                ServiceCallPatternEnum.FirstChance;

            var results = new List<ServiceCallResult>();

            var callQueue = CreateClientCallQueue(log, request.RequestorFabricId, targetHostId, useCallPattern);
            while (callQueue.Count > 0) {
                if (callQueue.TryDequeue(out var instClient)) {
                    var instResult = await instClient.ExecuteCall(useRequest);
                    results.Add(HydrateResult(request, instResult));
                    if (instResult.Completed) {
                        switch (instResult.OperationStatus) {
                        case LogResultStatusEnum.NoAction_BadRequest:
                        case LogResultStatusEnum.NonRetriableError:
                        case LogResultStatusEnum.Success:
                        case LogResultStatusEnum.PartialSuccess:
                        case LogResultStatusEnum.Pending:
                            return results;
                        default:
                            // in these cases, let the loop attempt the next in the queue
                            break;
                        }
                    }
                }
            }

            if (results.Count == 0) {
                var result = HydrateResult(
                    request,
                    null
                );
                result.ServiceCallStatus = ServiceCallStatusEnum.NoConnection;
                result.Message = "No connection to service";
                result.OperationStatus = LogResultStatusEnum.Incomplete;
                results.Add(result);
            }

            return results;
        }

        // =====================================================================
        // Private 
        // =====================================================================

        private static ServiceCallResult HydrateResult(
            ServiceCallRequest request,
            ServiceCallResult result = null
        ) { 
            if (result == null) {
                result = new ServiceCallResult {
                    ServiceCallStatus = ServiceCallStatusEnum.NoConnection
                };
            }
            result.CorrelationId = result?.CorrelationId ?? request.CorrelationId;
            result.OperationName = request.OperationName;
            result.RequestorFabricId = request.RequestorFabricId;
            result.RequestorInstanceId = request.RequestorInstanceId;
            return result;
        }

        private Queue<IInstanceClient> CreateClientCallQueue(
            ILogSession log,
            string requestingFabricId, 
            string targetHostId, 
            ServiceCallPatternEnum callPattern
        ) {

            if (
                callPattern == ServiceCallPatternEnum.SpecificHost && 
                string.IsNullOrEmpty(targetHostId)
            ) {
                var m = $"Inconsistent service call type.  {nameof(targetHostId)} must be specified if call pattern is {ServiceCallPatternEnum.SpecificHost}";
                log?.Erratum(m, new { requestor = requestingFabricId });
                throw new ArgumentException(m);
            }
            if (
                callPattern != ServiceCallPatternEnum.SpecificHost && 
                !string.IsNullOrEmpty(targetHostId)
            ) {
                var m = $"Inconsistent service call type.  {nameof(targetHostId)} can only be specified if call pattern is {ServiceCallPatternEnum.SpecificHost}";
                log?.Erratum(m, new { requestor = requestingFabricId });
                throw new ArgumentException(m);
            }

            // FUTURE:  Make these configurable

            // Note:  These are set up in an attempt to balance out the factors in relation
            // to each other and ensure "reachability" for any given service in the list.
            const int availabilityFactorWeight = 100;
            const int healthFactorWeight = (int)AvailabilityEnum.Serving5 * availabilityFactorWeight;
            const int sameHostFactorWeight = (int)AvailabilityEnum.Serving5 * availabilityFactorWeight;
            const int randomFactorRange = (int)AvailabilityEnum.Serving5 * availabilityFactorWeight;

            HashSet<string> instanceIdsAdded = new HashSet<string>(this.InstanceClients.Count);

            int getInstancePreference(IReadOnlyServiceInstance inst) =>
                // calculate a weight based on availability
                (int)inst.Status.Availability * availabilityFactorWeight +
                // add one factor if it's on the same host
                (inst.HostFabricId != requestingFabricId ? 0 : sameHostFactorWeight) +
                // add one factor if the health is above moderate or unknown
                (inst.Status.Health >= HealthEnum.Moderate || inst.Status.Health == HealthEnum.Unknown ? healthFactorWeight : 0) +
                // add a degree of randomness
                (Random.Next(randomFactorRange));

            bool canAddClientToQueue(IReadOnlyServiceInstance inst) => 
                    (targetHostId == null || inst.HostFabricId == targetHostId) &&
                    (inst.Status.Health >= HealthEnum.UnhealthyRecovering || inst.Status.Health == HealthEnum.Unknown) &&
                    inst.Status.Availability >= AvailabilityEnum.Serving5 &&
                    !instanceIdsAdded.Contains(inst.InstanceId);

            IInstanceClient selectInstanceClientAndMarkAsAdded(IInstanceClient client) {
                instanceIdsAdded.Add(client.Instance.InstanceId);
                return client;
            }

            return new Queue<IInstanceClient>(
                from 
                    c in InstanceClients
                where 
                    canAddClientToQueue(c.Instance) 
                orderby 
                    getInstancePreference(c.Instance) descending
                select 
                    selectInstanceClientAndMarkAsAdded(c)
            );
        }
    }
}

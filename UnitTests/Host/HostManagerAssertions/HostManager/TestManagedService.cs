using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.MetaServices;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Services.MessageBroker;

namespace UnitTests.Host.XKitHostAssertions.XKitHost {

    public interface ITestManagedService : IManagedService { }

    public class TestManagedService : ITestManagedService, IServiceBase, IServiceOperationOwner {

        private readonly HashSet<string> features = new();

        public Action HandleEnvironmentChange;
        public Action HandleHostStartupComplete;
        public Action HandleHostShutdownComplete;
        public AvailabilityEnum Availability { get; set; } = AvailabilityEnum.Serving9;
        public HealthEnum Health { get; set; } = HealthEnum.Healthy;

        // =====================================================================
        // IMetaService
        // =====================================================================

        public string InstanceId { get; set; } = XKit.Lib.Common.Utility.Identifiers.GenerateIdentifier();

        public IReadOnlyDescriptor Descriptor { get; set; } = new Descriptor {
            Collection = MetaServiceConstants.CollectionName,
            Name = nameof(TestMetaService),
            Version = 1,
            UpdateLevel = 0,
            PatchLevel = 0
        };

        public RunStateEnum ServiceState { get; set; } = RunStateEnum.Active;

        public string ServiceAddress { get; set; } = $"100.100.100.100";

        public IReadOnlyServiceCallPolicy CallPolicy =>
            throw new System.NotImplementedException();

        public string ConfigurationDocumentIdentifier =>
            throw new System.NotImplementedException();

        public IEnumerable<IReadOnlyDescriptor> Dependencies =>
            throw new System.NotImplementedException();
        public IEnumerable<IReadOnlySubscription> EventSubscriptions =>
            throw new System.NotImplementedException();
        public IEnumerable<IReadOnlySubscription> CommandSubscriptions =>
            throw new System.NotImplementedException();

        public ServiceInstanceStatus GetServiceStatus() => new() {
            InstanceId = this.InstanceId,
            RunState = this.ServiceState,
            Availability = this.Availability,
            Health = this.Health
        };

        public bool CanStartNewOperation() => this.ServiceState == RunStateEnum.Active;

        public void PauseService() => this.ServiceState = RunStateEnum.Paused;

        public void ResumeService() => this.ServiceState = RunStateEnum.Active;

        public void SignalEnvironmentChange() => HandleEnvironmentChange?.Invoke();

        public void SignalHostStartupComplete() => HandleHostStartupComplete?.Invoke();

        public void SignalHostShutdownComplete() => HandleHostShutdownComplete?.Invoke();

        public void StartService() => this.ServiceState = RunStateEnum.Active;

        public void StopService() => this.ServiceState = RunStateEnum.Inactive;

        IReadOnlyDescriptor IServiceBase.Descriptor =>
            throw new NotImplementedException();

        IReadOnlyServiceCallPolicy IServiceBase.CallPolicy =>
            throw new NotImplementedException();

        IEnumerable<IReadOnlyDescriptor> IServiceBase.Dependencies =>
            throw new NotImplementedException();

        IEnumerable<IReadOnlySubscription> IServiceBase.EventSubscriptions =>
            throw new NotImplementedException();

        IEnumerable<IReadOnlySubscription> IServiceBase.CommandSubscriptions =>
            throw new NotImplementedException();

        string IServiceBase.ConfigurationDocumentIdentifier =>
            throw new NotImplementedException();

        string IServiceBase.InstanceId =>
            throw new NotImplementedException();

        RunStateEnum IServiceBase.ServiceState =>
            throw new NotImplementedException();

        public IXKitHostEnvironment HostEnvironment => throw new NotImplementedException();

        public IXKitEnvironment XKitEnvironment => throw new NotImplementedException();

        public bool HasFeature(string featureName) {
            return features.Contains(featureName);
        }

        public void AddFeatureFlag(string featureName) {
            this.features.Add(featureName);
        }

        Task<ServiceCallResult> IServiceBase.ExecuteCall(
            ServiceCallRequest request
        ) {
            throw new NotImplementedException();
        }

        public Func<object, object[], object> GetInvoker(Type onType, string name) {
            throw new NotImplementedException();
        }

        ServiceInstanceStatus IServiceBase.GetServiceStatus() {
            return GetServiceStatus();
        }

        bool IServiceBase.CanStartNewOperation() {
            throw new NotImplementedException();
        }

        bool IServiceBase.HasFeature(string featureName) {
            throw new NotImplementedException();
        }

        void IServiceBase.SignalEnvironmentChange() {
            throw new NotImplementedException();
        }

        IServiceDaemon IServiceBase.GetDaemon(string daemonName) {
            throw new NotImplementedException();
        }

        IServiceDaemon[] IServiceBase.GetDaemons() {
            throw new NotImplementedException();
        }
    }
}

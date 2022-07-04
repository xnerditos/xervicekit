using System;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;

namespace XKit.Lib.Host.DefaultBaseClasses {

    public abstract class MetaService<TOperation>
        : ServiceBase<TOperation>, IMetaService
        where TOperation : IServiceOperation {

        // =====================================================================
        // protected
        // =====================================================================

        protected string CapabilityKeyName { get; }

        // =====================================================================
        // abstract and virtual members
        // =====================================================================
        protected override bool CanStartNewOperation()
            => (this.HostEnvironment.HostRunState == RunStateEnum.Active ||
               this.HostEnvironment.HostRunState == RunStateEnum.Paused) &&
               this.RunState == RunStateEnum.Active;

        // =====================================================================
        // Construction
        // =====================================================================

        public MetaService(
            string capabilityKeyName,
            IXkitHostEnvironment hostEnvironment
        ) : base(hostEnvironment) {
            this.CapabilityKeyName = capabilityKeyName ?? throw new ArgumentNullException(capabilityKeyName);
        }

        // =====================================================================
        // IMetaService
        // =====================================================================

        string IMetaService.CapabilityKeyName => this.CapabilityKeyName;
    }
}

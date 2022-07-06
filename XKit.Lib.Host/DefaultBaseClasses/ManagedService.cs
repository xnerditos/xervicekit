using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Host;

namespace XKit.Lib.Host.DefaultBaseClasses {

    public abstract class ManagedService<TOperation> 
        : ServiceBase<TOperation>, IManagedService
        where TOperation : IServiceOperation {

        // =====================================================================
        // Protected members
        // =====================================================================
                        
        protected string LocalDataFolderPath { get; private set; }

        protected override void StartService() {
            LocalDataFolderPath = $"{HostEnvironment.DataRootFolderPath}/{Descriptor.Collection}/{Descriptor.Name}/{Descriptor.Version}";
            base.StartService();
        }
        
        // =====================================================================
        // abstract and virtual members
        // =====================================================================
        
        protected virtual AvailabilityEnum GetAvailability() 
            => AvailabilityEnum.Serving5;
        
        protected override bool CanStartNewOperation() {
            switch (HostEnvironment.HostRunState) {
            case RunStateEnum.Inactive:
            case RunStateEnum.Paused:
            case RunStateEnum.ShuttingDown:
            case RunStateEnum.Unknown:
                return false;
            default:
                return this.RunState == RunStateEnum.Active;
            }
        }

        // =====================================================================
        // events
        // =====================================================================

        // =====================================================================
        // Construction
        // =====================================================================

        protected ManagedService(
            IXKitHostEnvironment hostEnvironment
        ) : base(hostEnvironment) { }

        // =====================================================================
        // IManagedService
        // =====================================================================

        void IManagedService.PauseService() 
            => PauseService();

        void IManagedService.ResumeService() 
            => ResumeService();
    }
}

using XKit.Lib.Common.Host;
using XKit.Lib.Common.MetaServices;
using XKit.Lib.Common.Registration;
using XKit.Lib.Host.DefaultBaseClasses;
using XKit.Lib.Common.Fabric;

namespace XKit.Lib.Host.MetaServices.RegistrationsManagement {

    public interface IRegistrationsManagementMetaService 
        : IMetaService, IServiceBase { }

    internal class RegistrationManagementMetaService 
        : MetaService<RegistrationsManagementOperation>, IRegistrationsManagementMetaService {

        public RegistrationManagementMetaService(
            IXKitHost localHost
        ) : base(
                StandardCapabilityNames.LocalRegistrationsManagement,
                localHost
            ) {
            }

        // =====================================================================
        // base class overrides
        // =====================================================================

        protected override IReadOnlyDescriptor Descriptor => MetaServiceConstants.Services.RegistrationsManagement.Descriptor;
    }
}

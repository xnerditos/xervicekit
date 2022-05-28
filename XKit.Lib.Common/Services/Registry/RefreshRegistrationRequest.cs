using System.Collections.Generic;
using XKit.Lib.Common.Registration;

namespace XKit.Lib.Common.Services.Registry {
    public class RefreshRegistrationRequest {

        public string FabricId { get; set; }

        public FabricStatus UpdateStatus { get; set; }

        public List<ServiceInstanceStatus> UpdateServiceStatuses { get; set; }

        public List<Descriptor> NewDependencies { get; set; }
    }
}
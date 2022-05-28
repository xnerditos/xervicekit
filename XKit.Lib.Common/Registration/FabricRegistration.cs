using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace XKit.Lib.Common.Registration {

	public partial class FabricRegistration : IReadOnlyFabricRegistration {

		public string FabricId { get; set; }

        public FabricStatus Status { get; set; }

		public List<ServiceRegistration> HostedServices { get; set; }

		public List<Descriptor> Dependencies { get; set; }

		public string Address { get; set; }

		public List<string> Capabilities { get; set; }

        public FabricRegistration Clone() 
            => new() {
                Dependencies = Dependencies?.Select(x => x.Clone()).ToList(),
                Capabilities = new List<string>(Capabilities),
                Address = Address,
                FabricId = FabricId,
                Status = Status?.Clone(),
                HostedServices = HostedServices?.Select(x => x.Clone()).ToList()
            };

        [JsonIgnore]
		IEnumerable<IReadOnlyDescriptor> IReadOnlyFabricRegistration.Dependencies => this.Dependencies;

        [JsonIgnore]
        IEnumerable<IReadOnlyServiceRegistration> IReadOnlyFabricRegistration.HostedServices => this.HostedServices;

        [JsonIgnore]
        IReadOnlyFabricStatus IReadOnlyFabricRegistration.Status => Status;

        [JsonIgnore]
        IEnumerable<string> IReadOnlyFabricRegistration.Capabilities => Capabilities;
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace XKit.Lib.Common.Registration {

	public partial class ServiceRegistration : IReadOnlyServiceRegistration {

		public string RegistrationKey { get; set; }

		public Descriptor Descriptor { get; set; }

        public List<ServiceInstance> Instances { get; set; }

		public ServiceCallPolicy CallPolicy { get; set; }

        public ServiceRegistration Clone() 
            => new() {
                Descriptor = Descriptor?.Clone(),
                CallPolicy = CallPolicy?.Clone(),
                Instances = Instances?.Select(inst => inst.Clone()).ToList(),
                RegistrationKey = RegistrationKey
            };

        [JsonIgnore]
		IReadOnlyDescriptor IReadOnlyServiceRegistration.Descriptor => this.Descriptor;

        [JsonIgnore]
		IEnumerable<IReadOnlyServiceInstance> IReadOnlyServiceRegistration.Instances => this.Instances;

        [JsonIgnore]
		IReadOnlyServiceCallPolicy IReadOnlyServiceRegistration.CallPolicy => this.CallPolicy;
	}
}

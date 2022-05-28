using System.Text.Json.Serialization;

namespace XKit.Lib.Common.Registration {

	public partial class ServiceInstance : IReadOnlyServiceInstance {

        public Descriptor Descriptor { get; set; }

		public string InstanceId { get; set; }

		public string HostFabricId { get; set; }

        public string HostAddress { get; set; }

		public string RegistrationKey { get; set; }

		public ServiceInstanceStatus Status { get; set; }

        public ServiceInstance Clone() 
            => new() {
                HostFabricId = HostFabricId,
                InstanceId = InstanceId,
                RegistrationKey = RegistrationKey,
                HostAddress = HostAddress,
                Status = Status?.Clone(),
                Descriptor = Descriptor?.Clone()
            };

        [JsonIgnore]
		IReadOnlyServiceInstanceStatus IReadOnlyServiceInstance.Status => this.Status;
        [JsonIgnore]
        IReadOnlyDescriptor IReadOnlyServiceInstance.Descriptor => this.Descriptor;
	}
}

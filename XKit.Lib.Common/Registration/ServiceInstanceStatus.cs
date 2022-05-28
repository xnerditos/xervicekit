using System.Text.Json.Serialization;

namespace XKit.Lib.Common.Registration {

	public partial class ServiceInstanceStatus : IReadOnlyServiceInstanceStatus {

		public string InstanceId { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
		public AvailabilityEnum Availability { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
		public HealthEnum Health { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
		public RunStateEnum RunState { get; set; }

        public ServiceInstanceStatus Clone() 
            => new() {
                Availability = Availability,
                Health = Health,
                InstanceId = InstanceId,
                RunState = RunState
            };

		public void TryUpdate(IReadOnlyServiceInstanceStatus update) 
			=> TryUpdate(
				update.Availability,
				update.Health,
				update.RunState
			);

		public void TryUpdate(AvailabilityEnum? availability, HealthEnum? health, RunStateEnum? state = null) {
			if (availability.HasValue) {
				this.Availability = availability.Value;
			}
			if (health.HasValue) {
				this.Health = health.Value;
			}
			if (state.HasValue) {
				this.RunState = state.Value;
			}
		}
	}
}

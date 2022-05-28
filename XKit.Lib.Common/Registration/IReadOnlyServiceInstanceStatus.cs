using System.Text.Json.Serialization;

namespace XKit.Lib.Common.Registration {
	public interface IReadOnlyServiceInstanceStatus {
		string InstanceId { get; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
		AvailabilityEnum Availability { get; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
		HealthEnum Health { get; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
		RunStateEnum RunState { get; }

        ServiceInstanceStatus Clone();
	}
}

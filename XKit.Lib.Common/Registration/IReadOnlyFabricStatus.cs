
using System.Text.Json.Serialization;

namespace XKit.Lib.Common.Registration {
	public interface IReadOnlyFabricStatus {
		string FabricId { get; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
		HealthEnum? Health { get; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
		RunStateEnum? RunState { get; }

        FabricStatus Clone();
	}
}

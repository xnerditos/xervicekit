using System.Text.Json.Serialization;

namespace XKit.Lib.Common.Registration {

	public partial class FabricStatus : IReadOnlyFabricStatus {

		public string FabricId { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
		public HealthEnum? Health { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
		public RunStateEnum? RunState { get; set; }

        public FabricStatus Clone() 
            => new() {
                Health = Health,
                FabricId = FabricId,
                RunState = RunState
            };
            
		public void TryUpdate(HealthEnum? health, RunStateEnum? state = null) {
			if (health.HasValue) {
				this.Health = health.Value;
			}
			if (state.HasValue) {
				this.RunState = state.Value;
			}
		}
	}
}

using System.Text.Json.Serialization;

namespace XKit.Lib.Common.Registration {

	public partial class ServiceCallPolicy : IReadOnlyServiceCallPolicy {
		
        [JsonConverter(typeof(JsonStringEnumConverter))]
		public ServiceCallPatternEnum? CallPattern { get; set; }

        public ServiceCallPolicy Clone() 
            => new() {
                CallPattern = CallPattern
            };
	}
}

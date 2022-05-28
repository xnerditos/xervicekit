using System.Text.Json.Serialization;

namespace XKit.Lib.Common.Registration {

	public interface IReadOnlyServiceCallPolicy  {

        [JsonConverter(typeof(JsonStringEnumConverter))]
		ServiceCallPatternEnum? CallPattern { get; }

        ServiceCallPolicy Clone();
	}
}

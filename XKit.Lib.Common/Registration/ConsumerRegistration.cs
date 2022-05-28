using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Linq;

namespace XKit.Lib.Common.Registration {

	public partial class ConsumerRegistration : IReadOnlyConsumerRegistration {

		public string FabricId { get; set; }

		public List<Descriptor> Dependencies { get; set; }

        public ConsumerRegistration Clone() 
            => new() {
                Dependencies = Dependencies.Select(x => x.Clone()).ToList(),
                FabricId = FabricId
            };

        [JsonIgnore]
		IEnumerable<IReadOnlyDescriptor> IReadOnlyConsumerRegistration.Dependencies => this.Dependencies;
    }
}

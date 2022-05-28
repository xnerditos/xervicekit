using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace XKit.Lib.Common.Registration {

	public partial class ServiceTopologyMap : IReadOnlyServiceTopologyMap {

		public List<ServiceRegistration> Services { get; set; }

		public DateTime? CacheExpiration { get; set; }
        
        public ServiceTopologyMap Clone() 
            => new() {
                CacheExpiration = CacheExpiration,
                Services = Services?.Select(x => x.Clone()).ToList()
            };

        [JsonIgnore]
		IEnumerable<IReadOnlyServiceRegistration> IReadOnlyServiceTopologyMap.Services => this.Services;
	}
}

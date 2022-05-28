using System.Collections.Generic;
using XKit.Lib.Common.Host;

namespace XKit.Lib.Common.Services.Config {
    public class ConfigServiceQueryResponse {
        public HostConfigDocument HostConfig { get; set; }
        public Dictionary<string, string> ServiceConfigJson { get; set; }
    }
}

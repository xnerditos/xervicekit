using System.Collections.Generic;

namespace XKit.Lib.Common.Services.Config {
    public class ConfigServiceQueryRequest {
        public int? HostVersionLevel { get; set; }
        public string[] ServiceKeys { get; set; }
    }
}
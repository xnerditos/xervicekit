using System.Collections.Generic;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Utility;
using XKit.Lib.Common.Utility.Extensions;
using XKit.Lib.Host.DefaultBaseClasses;

namespace XKit.Lib.Testing.TestConfigSvc {

    public class ConfigSvcService : ManagedService<ConfigSvcOperation>, IConfigSvcService {

        private static readonly Dictionary<string, string> ConfigObjects = new();

        private static readonly IReadOnlyDescriptor descriptor = XKit.Lib.Common.Services.StandardConstants.Managed.StandardServices.Config.Descriptor;

        public static void ClearAllExisting() {
            ConfigObjects.Clear();
        }

        public static void SetConfigForService(IReadOnlyDescriptor descriptor, object configObject) {
            ConfigObjects[Common.Utility.Identifiers.GetServiceVersionLevelKey(descriptor)] = configObject.ToJson(pretty: true);
        }

        public static void SetConfigForHost(HostConfigDocument configObject) {
            ConfigObjects[string.Empty] = configObject.ToJson(pretty: true);
        }

        // =====================================================================
        // overrides
        // =====================================================================

        protected override IReadOnlyDescriptor Descriptor => descriptor;

        protected override IEnumerable<IReadOnlyDescriptor> Dependencies => System.Array.Empty<IReadOnlyDescriptor>();

        protected override IReadOnlyServiceCallPolicy CallPolicy => DefaultPolicy;

        protected override string ConfigurationDocumentIdentifier => Identifiers.GetServiceVersionLevelKey(descriptor);

        // =====================================================================
        // construction
        // =====================================================================

        public ConfigSvcService(
            IXkitHostEnvironment hostEnv
        ) : base(hostEnv) { }

        // =====================================================================
        // GetConfigForService
        // =====================================================================

        string IConfigSvcService.GetConfigJsonForService(string serviceKey) {
            ConfigObjects.TryGetValue(serviceKey, out var json);
            return json;
        }

        HostConfigDocument IConfigSvcService.GetConfigForHost() {
            ConfigObjects.TryGetValue(string.Empty, out var json);
            return json.FromJson<HostConfigDocument>() ?? new HostConfigDocument();
        }
    }
}

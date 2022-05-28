using System.Threading.Tasks;
using XKit.Lib.Common.Config;
using System.IO;
using System.Text;
using XKit.Lib.Common.Utility.Extensions;

namespace XKit.Lib.Host.Config {
    internal class LocalConfigSession : ILocalConfigSession {
        
        private string configFile;

        public LocalConfigSession(
            string configFile
        ) {
            this.configFile = configFile;
        }

        async Task<object> ILocalConfigSession.GetConfig(object defaultConfig) {
            var tries = 5;
            while(tries-- > 0) {
                try {
                    if (File.Exists(configFile)) {
                        string jsonStr = await File.ReadAllTextAsync(configFile);
                        return jsonStr.FromJsonDynamic();
                    }
                    return defaultConfig?.DeepCopy();
                } catch {
                    // eat exception
                }             
            }
            return defaultConfig?.DeepCopy();
        }

        async Task<T> ILocalConfigSession.GetConfig<T>(T defaultConfig) {
            var tries = 5;
            while(tries-- > 0) {
                try {
                    if (File.Exists(configFile)) {
                        string jsonStr = await File.ReadAllTextAsync(configFile);
                        return jsonStr.FromJson<T>();
                    }
                    return defaultConfig?.DeepCopy();
                } catch {
                    // eat exception
                }             
            }
            return defaultConfig?.DeepCopy();
        }

        async Task ILocalConfigSession.UpdateConfig(object configEntity) {
            var tries = 5;
            while(tries-- > 0) {
                try {
                    string jsonStr = configEntity?.ToJson(pretty: true) ?? "{ }";
                    await File.WriteAllTextAsync(configFile, jsonStr, Encoding.UTF8);
                    return;
                } catch {
                    // eat exception
                }             
            }
        }

        async Task ILocalConfigSession.UpdateConfigJson(string json) {
            var tries = 5;
            while(tries-- > 0) {
                try {
                    await File.WriteAllTextAsync(configFile, json, Encoding.UTF8);
                    return;
                } catch {
                    // eat exception
                }             
            }
        }

        async Task ILocalConfigSession.UpdateConfig<T>(T configEntity) {
            var tries = 5;
            while(tries-- > 0) {
                try {
                    string jsonStr = Json.To(configEntity);
                    await File.WriteAllTextAsync(configFile, jsonStr, Encoding.UTF8);
                    return;
                } catch {
                     // eat exception
                }             
            }
        }
    }
}
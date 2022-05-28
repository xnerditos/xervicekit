using XKit.Lib.Common.Utility.Extensions;
using XKit.Lib.Common.Config;
using XKit.Lib.Common.Utility.Threading;
using System;

namespace XKit.Lib.Host.Config {

    public class ConfigReader<TConfig> : IConfigReader<TConfig> where TConfig : class, new() {

        private const int ConfigCacheSecondsBase = 60;
        private const int ConfigCacheSecondsRandom = 60;
        private string configDocumentIdentifier;
        private ILocalConfigSessionFactory localConfigSessionFactory;
        private string extendedConfigName;
        private TConfig cachedConfig;
        private DateTime configReloadTime = DateTime.MinValue;
        
        // FUTURE:  Use default at the property/field level by doing a "merge copy" between the retrieved config and
        //         the default object

        public ConfigReader(
            string configDocumentIdentifier,
            string extendedConfigName,
            ILocalConfigSessionFactory localConfigSessionFactory
        ) {
            this.configDocumentIdentifier = configDocumentIdentifier;
            this.extendedConfigName = extendedConfigName;
            this.localConfigSessionFactory = localConfigSessionFactory;
        }

        private TConfig GetConfig(TConfig defaultValue) {
            if (cachedConfig == null || configReloadTime < DateTime.UtcNow) {
                var localConfigSession = localConfigSessionFactory.Create(configDocumentIdentifier, extendedConfigName);
                cachedConfig = TaskUtil.RunSyncSafely(() => localConfigSession.GetConfig<TConfig>());
                configReloadTime = DateTime.UtcNow.AddSeconds(ConfigCacheSecondsBase + Math.Abs(this.GetHashCode() % ConfigCacheSecondsRandom));
            }
            return cachedConfig ?? defaultValue?.DeepCopy() ?? new TConfig();
        }

        // =====================================================================
        // IConfigReader
        // =====================================================================
        
        void IConfigReader.InvalidateCache()
            => cachedConfig = null;

        TConfig IConfigReader<TConfig>.GetConfig(TConfig defaultValue) 
            => GetConfig(defaultValue);

        object IConfigReader.GetConfig(object defaultValue) 
            => GetConfig((TConfig)defaultValue);
    }
}

using System;
using System.Threading.Tasks;

namespace XKit.Lib.Common.Config {

    /// <summary>
    /// Manages config information storage and retrieval local to the process
    /// </summary>
    public interface ILocalConfigSession {
        Task UpdateConfig(object configEntity);
        Task UpdateConfigJson(string configJson);
        Task UpdateConfig<T>(T configEntity) where T : class, new();
        Task<object> GetConfig(object defaultConfig = null);
        Task<T> GetConfig<T>(T defaultConfig = default(T)) where T : class, new();
    }
}

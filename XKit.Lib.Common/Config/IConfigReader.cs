
namespace XKit.Lib.Common.Config {

    /// <summary>
    /// Responsible for retrieving the config info for a service. 
    /// </summary>
    public interface IConfigReader {
        void InvalidateCache();
        object GetConfig(object defaultValue = null);
    }

    /// <summary>
    /// Responsible for retrieving the config info for a service. 
    /// </summary>
    /// <typeparam name="TConfig">The config entity type</typeparam>
    public interface IConfigReader<TConfig> : IConfigReader where TConfig : class, new() {
        TConfig GetConfig(TConfig defaultValue = default(TConfig));
    }
}

using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Config;
using XKit.Lib.Common.Utility;

namespace XKit.Lib.Host.Config {
    public interface IConfigReaderFactory {
        IConfigReader<T> CreateForService<T>(
			IReadOnlyDescriptor service,
			ILocalConfigSessionFactory configSessionFactory,
            string extendedName = null
		) where T : class, new();


        IConfigReader<T> CreateForHost<T>(
			int hostVersionLevel,
			ILocalConfigSessionFactory configSessionFactory,
            string extendedName
		) where T : class, new();    }
    
	public class ConfigReaderFactory : IConfigReaderFactory {

		private static IConfigReaderFactory factory = new ConfigReaderFactory();
		public static IConfigReaderFactory Factory => factory;

		// =====================================================================
		// IConfigReaderFactory
		// =====================================================================

		IConfigReader<T> IConfigReaderFactory.CreateForService<T>(
            IReadOnlyDescriptor service,
			ILocalConfigSessionFactory configSessionFactory,
            string extendedName
        ) => new ConfigReader<T>(
            Identifiers.GetServiceVersionLevelKey(service), 
            extendedName,
            configSessionFactory
        );

		IConfigReader<T> IConfigReaderFactory.CreateForHost<T>(
            int hostVersionLevel,
			ILocalConfigSessionFactory configSessionFactory,
            string extendedName
        ) => new ConfigReader<T>(
            Identifiers.GetHostVersionLevelKey(hostVersionLevel), 
            extendedName,
            configSessionFactory
        );

		// =====================================================================
		// Static
		// =====================================================================

		public static IConfigReader<T> CreateForService<T>(
			IReadOnlyDescriptor service,
			ILocalConfigSessionFactory configSessionFactory,
            string extendedName = null
		) where T : class, new() => Factory.CreateForService<T>(service, configSessionFactory, extendedName);

		public static IConfigReader<T> CreateForHost<T>(
			int hostVersionLevel,
			ILocalConfigSessionFactory configSessionFactory,
            string extendedName = null
		) where T : class, new() => Factory.CreateForHost<T>(hostVersionLevel, configSessionFactory, extendedName);

		public static void InjectCustomFactory(IConfigReaderFactory factory) 
			=> ConfigReaderFactory.factory = factory;
    }
}
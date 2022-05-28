using XKit.Lib.Common.Log;

namespace XKit.Lib.LocalLog {

    public class LogSessionFactory : ILogSessionFactory {

		private static ILogSessionFactory factory = new LogSessionFactory();
		public static ILogSessionFactory Factory => factory;

        private ILogWriter logWriter = new ConsoleLogWriter();

		// =====================================================================
		// ILogSessionFactory
		// =====================================================================

        void ILogSessionFactory.SetLogWriter(ILogWriter logWriter) {
            this.logWriter = logWriter;
        }

        ILogSession ILogSessionFactory.CreateLogSession(
            string originatorName, 
            int? originatorVersion, 
            string originatorFabricId, 
            string originatorInstanceId, 
            string correlationId
        ) => new LogSession(
            logWriter, 
            originatorName,
            originatorVersion,
            originatorFabricId,
            originatorInstanceId,
            correlationId
        );

		// =====================================================================
		// Static
		// =====================================================================

		public static void InjectCustomFactory(ILogSessionFactory factory) 
			=> LogSessionFactory.factory = factory;
    }
}

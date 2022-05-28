
namespace XKit.Lib.Common.Log {

    public interface ILogSessionFactory {

        void SetLogWriter(ILogWriter logWriter);

        ILogSession CreateLogSession(
            string originatorName = null, 
            int? originatorVersion = null, 
            string originatorFabricId = null, 
            string originatorInstanceId = null, 
            string correlationId = null
        );
    }
}

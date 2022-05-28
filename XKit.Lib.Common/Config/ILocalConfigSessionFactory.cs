
namespace XKit.Lib.Common.Config {
    public interface ILocalConfigSessionFactory {
        void SetPath(
            string localConfigFolder
        );
        
        ILocalConfigSession Create(
            string configDocumentIdentifier,
            string extendedName = null
        );
    }
}
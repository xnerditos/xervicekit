namespace XKit.Lib.Common.Utility.ObjectRepository {

    public interface IObjectRepositoryFactory{
        IObjectRepository CreateSingleton();
        IObjectRepository Create();
    }

    public class ObjectRepositoryFactory : IObjectRepositoryFactory {

        private static IObjectRepositoryFactory factory = new ObjectRepositoryFactory();

        private static IObjectRepository singleton;

        public static IObjectRepositoryFactory Factory => factory;

        // =====================================================================
        // IInjectableGlobalObjectRepositoryFactory
        // =====================================================================

        IObjectRepository IObjectRepositoryFactory.CreateSingleton() {
            if (singleton == null) {
                singleton = new ObjectRepository();
            }
            return singleton;
        }

        IObjectRepository IObjectRepositoryFactory.Create() {
            return new ObjectRepository();
        }

        // =====================================================================
        // static
        // =====================================================================

        public static IObjectRepository CreateSingleton() 
            => factory.CreateSingleton();

        public static IObjectRepository Create() 
            => factory.Create();

        public static void InjectCustomFactory(IObjectRepositoryFactory factory) 
            => ObjectRepositoryFactory.factory = factory;
    }
}

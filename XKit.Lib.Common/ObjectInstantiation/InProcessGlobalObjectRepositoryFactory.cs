namespace XKit.Lib.Common.ObjectInstantiation {

    public interface IInProcessGlobalObjectRepositoryFactory{
        IInProcessGlobalObjectRepository CreateSingleton();
        
    }

    public class InProcessGlobalObjectRepositoryFactory : IInProcessGlobalObjectRepositoryFactory {

        private static IInProcessGlobalObjectRepositoryFactory factory = new InProcessGlobalObjectRepositoryFactory();

        private static IInProcessGlobalObjectRepository singleton;

        public static IInProcessGlobalObjectRepositoryFactory Factory => factory;

        // =====================================================================
        // IInjectableGlobalObjectRepositoryFactory
        // =====================================================================

        IInProcessGlobalObjectRepository IInProcessGlobalObjectRepositoryFactory.CreateSingleton() {
            if (singleton == null) {
                singleton = new InProcessGlobalObjectRepository();
            }
            return singleton;
        }

        // =====================================================================
        // static
        // =====================================================================

        public static IInProcessGlobalObjectRepository CreateSingleton() 
            => factory.CreateSingleton();

        public static void InjectCustomFactory(IInProcessGlobalObjectRepositoryFactory factory) 
            => InProcessGlobalObjectRepositoryFactory.factory = factory;
    }
}
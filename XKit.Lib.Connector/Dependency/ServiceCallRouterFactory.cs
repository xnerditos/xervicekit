using System;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Registration;
using System.Collections.Generic;

namespace XKit.Lib.Connector.Dependency {

    public interface IServiceCallRouterFactory {
        IServiceCallRouter Create(
            IReadOnlyServiceRegistration targetService,
            DateTime validUntilTime,
            IEnumerable<IInstanceClient> instanceClients
        );
    }

    public class ServiceCallRouterFactory : IServiceCallRouterFactory {

        private static IServiceCallRouterFactory factory = new ServiceCallRouterFactory();
        
        public static IServiceCallRouterFactory Factory => factory;
        
        // =====================================================================
        // IDependencyClientFactory default implementations
        // =====================================================================
        
        IServiceCallRouter IServiceCallRouterFactory.Create(
            IReadOnlyServiceRegistration targetService,
            DateTime validUntilTime,
            IEnumerable<IInstanceClient> instanceClients
        ) {
            return new ServiceCallRouter(
                targetService,
                validUntilTime,
                instanceClients
            );
        }

        // =====================================================================
        // Static methods
        // =====================================================================

        public static IServiceCallRouter Create(
            ServiceRegistration targetService,
            DateTime validUntilTime,
            IEnumerable<IInstanceClient> instanceClients
        ) => Factory.Create(
            targetService,
            validUntilTime,
            instanceClients
        );

        public static void InjectCustomFactory(IServiceCallRouterFactory factory) =>
            ServiceCallRouterFactory.factory = factory; 
    }
}

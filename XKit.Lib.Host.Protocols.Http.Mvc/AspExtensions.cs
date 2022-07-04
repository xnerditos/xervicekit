using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using XKit.Lib.Common.Host;
using XKit.Lib.Host.Protocols.Http.Mvc.Helpers;

namespace XKit.Lib.Host.Protocols.Http.Mvc {
    public static class AspExtensions {

        public static IMvcBuilder AddApplicationType<T>(this IMvcBuilder builder) 
            => builder.AddApplicationPart(
                typeof(T)
                    .GetTypeInfo().Assembly
            );
        
        public static IMvcCoreBuilder AddApplicationType<T>(this IMvcCoreBuilder builder) 
            => builder.AddApplicationPart(
                typeof(T)
                    .GetTypeInfo().Assembly
            );

        public static IMvcBuilder AddXKitHostMvc(this IMvcBuilder builder) {

            return builder.AddApplicationPart(
                typeof(AspExtensions)
                    .GetTypeInfo().Assembly
            );
        }

        public static IMvcCoreBuilder AddXKitHostMvc(this IMvcCoreBuilder builder) {
            
            return builder.AddApplicationPart(
                typeof(AspExtensions)
                    .GetTypeInfo().Assembly
            );
        }
    }
}

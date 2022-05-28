using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace XKit.Lib.Host.Protocols.Http.Mvc.Helpers {
    public class ServiceControllerWiringFeatureProvider : IApplicationFeatureProvider<ControllerFeature> {

        private static Assembly serviceControllerAssembly;        

        public static void SetServiceControllerAssembly(Assembly serviceControllerAssembly) {
            ServiceControllerWiringFeatureProvider.serviceControllerAssembly = serviceControllerAssembly;
        }
        
        void IApplicationFeatureProvider<ControllerFeature>.PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature) {
            
            var candidates = serviceControllerAssembly.GetExportedTypes();
            var baseControllerType = typeof(ServiceControllerBase);
            foreach (var candidate in candidates)
            {
                if (candidate.IsSubclassOf(baseControllerType)) {
                    feature.Controllers.Add(candidate.GetTypeInfo());
                }
            }        
        }
    }
}
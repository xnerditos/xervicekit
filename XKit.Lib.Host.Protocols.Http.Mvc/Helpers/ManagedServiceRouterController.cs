using System;
using System.Linq;
using System.Threading.Tasks;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.ObjectInstantiation;
using Microsoft.AspNetCore.Mvc;

namespace XKit.Lib.Host.Protocols.Http.Mvc.Helpers {

    [Route("managed")]
    public class ManagedServiceRouterController : ServiceControllerBase {

        public ManagedServiceRouterController() : base(
            InProcessGlobalObjectRepositoryFactory.CreateSingleton().GetObject<IHostEnvironment>()
        ) { }

        [HttpPost("{collectionName}/{serviceName}/{serviceVersion}")]
        public async Task<IActionResult> RouteServiceRequest(
            [FromRoute] string collectionName,
            [FromRoute] string serviceName,
            [FromRoute] int serviceVersion
        ) {
            var svc = LocalEnvironment.GetManagedServices(
                collectionName,
                serviceName,
                serviceVersion
            ).SingleOrDefault();

            return await RunServiceOperationAndHttpResponse(
                svc
            );
        }
    }
}

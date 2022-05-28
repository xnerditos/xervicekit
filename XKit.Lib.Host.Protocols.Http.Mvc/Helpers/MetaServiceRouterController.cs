using System.Linq;
using System.Threading.Tasks;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.ObjectInstantiation;
using Microsoft.AspNetCore.Mvc;

namespace XKit.Lib.Host.Protocols.Http.Mvc.Helpers {

    [Route("meta")]
    public class MetaServiceRouterController : ServiceControllerBase {

        protected MetaServiceRouterController(
        ) : base(
            InProcessGlobalObjectRepositoryFactory.CreateSingleton().GetObject<IHostEnvironment>()
        ) {
        }

        [HttpPost("{collectionName}/{serviceName}/{serviceVersion}")]
        public async Task<IActionResult> ExecuteOperationRequest(
            //[FromRoute] string collectionName,
            [FromRoute] string serviceName
            //[FromRoute] int serviceVersion
        ) {
            var svc = LocalEnvironment.GetMetaServices(
                serviceName
            ).FirstOrDefault();
            
            return await RunServiceOperationAndHttpResponse(
                svc
            );
        }
    }
}

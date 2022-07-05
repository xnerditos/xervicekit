using System.Linq;
using System.Threading.Tasks;
using XKit.Lib.Common.Host;
using Microsoft.AspNetCore.Mvc;

namespace XKit.Lib.Host.Protocols.Http.Helpers {

    [Route("meta")]
    public class MetaServiceRouterController : ServiceControllerBase {

        protected MetaServiceRouterController(IXkitHostEnvironment hostEnvironment) 
            : base(hostEnvironment) {
        }

        [HttpPost("{collectionName}/{serviceName}/{serviceVersion}")]
        public async Task<IActionResult> ExecuteOperationRequest(
            //[FromRoute] string collectionName,
            [FromRoute] string serviceName
            //[FromRoute] int serviceVersion
        ) {
            var svc = HostEnvironment.GetMetaServices(
                serviceName
            ).FirstOrDefault();
            
            return await RunServiceOperationAndHttpResponse(
                svc
            );
        }
    }
}

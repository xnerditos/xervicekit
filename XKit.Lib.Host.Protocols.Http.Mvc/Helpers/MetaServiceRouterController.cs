using System.Linq;
using System.Threading.Tasks;
using XKit.Lib.Common.Host;
using Microsoft.AspNetCore.Mvc;

namespace XKit.Lib.Host.Protocols.Http.Mvc.Helpers {

    [Route("meta")]
    public class MetaServiceRouterController : ServiceControllerBase {

        private static IHostEnvironment hostEnvironment;

        public static void SetHostEnvironment(IHostEnvironment hostEnvironment) {
            MetaServiceRouterController.hostEnvironment = hostEnvironment;
        }

        protected MetaServiceRouterController(
        ) : base(
            hostEnvironment
        ) {
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

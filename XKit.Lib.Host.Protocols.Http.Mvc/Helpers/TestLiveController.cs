using Microsoft.AspNetCore.Mvc;
using XKit.Lib.Common.Host;

namespace XKit.Lib.Host.Protocols.Http.Mvc.Helpers {

    [Route("testlive")]
    public class TestLiveController : ControllerBase {

        private readonly IXkitHostEnvironment hostEnvironment;

        // public static void SetHostEnvironment(IXkitHostEnvironment hostEnvironment) {
        //     TestLiveController.hostEnvironment = hostEnvironment;
        // }

        public TestLiveController(IXkitHostEnvironment hostEnvironment) {
            this.hostEnvironment = hostEnvironment;
        }

        [HttpGet]
        public IActionResult ExecuteOperationRequest() {
            var version = hostEnvironment?.VersionLevel?.ToString();
            return Ok(
                "host is running.  Version: " + version ?? "<NULL>"
            );
        }
    }
}

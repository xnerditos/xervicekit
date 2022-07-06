using Microsoft.AspNetCore.Mvc;
using XKit.Lib.Common.Host;

namespace XKit.Lib.Host.Protocols.Http.Helpers {

    [Route("testlive")]
    public class TestLiveController : ControllerBase {

        private readonly IXKitHostEnvironment hostEnvironment;

        // public static void SetHostEnvironment(IXKitHostEnvironment hostEnvironment) {
        //     TestLiveController.hostEnvironment = hostEnvironment;
        // }

        public TestLiveController(IXKitHostEnvironment hostEnvironment) {
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

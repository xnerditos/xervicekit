using System;
using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using Microsoft.AspNetCore.Mvc;
using XKit.Lib.Common.Host;
using System.IO;
using XKit.Lib.Common.Utility.Extensions;

namespace XKit.Lib.Host.Protocols.Http.Helpers {

    [Produces("application/json")]
    [ApiController]
    public abstract class ServiceControllerBase : ControllerBase {

        protected IXKitHostEnvironment HostEnvironment { get; private set; }
        protected ServiceOperationContext Context { get; private set; }
        protected IServiceBase ServiceCore { get; private set; } 
        protected IServiceOperation Operation { get; private set; }

        protected ServiceControllerBase(
            IXKitHostEnvironment xkitEnvironment,
            IServiceBase service
        ) {
            HostEnvironment = xkitEnvironment ?? throw new ArgumentNullException(nameof(xkitEnvironment));
            ServiceCore = service ?? throw new ArgumentNullException(nameof(service));
        }

        protected ServiceControllerBase(
            IXKitHostEnvironment hostEnvironment
        ) {
            HostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
        }

        protected void SetService(
            IServiceBase service
        ) {
            ServiceCore = service ?? throw new ArgumentNullException(nameof(service));
        }

        protected async Task<ActionResult> RunServiceOperationAndHttpResponse(
            IServiceBase service
        ) {
            if (service == null) {
                return BadRequest("Service not found");
            }
            SetService(service);

            using var reader = new StreamReader(Request.Body);
            string content = await reader.ReadToEndAsync();
            HttpServiceCallRequest request = Json.FromJson<HttpServiceCallRequest>(content);                        

            ServiceCallResult result = null;
            Exception operationException = null;
            try { 
                result = await service.ExecuteCall(request);
            } catch(Exception ex) { 
                operationException = ex; 
            }
            
            if (request.UsesPayloadObj) {
                return Ok(HttpServiceCallResultUsingPayloadObj.CreateFrom(result));
            } else {
                return Ok(result);
            }
        }
    }
}

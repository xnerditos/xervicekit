using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Utility.Extensions;

namespace XKit.Lib.Host.Protocols.Http.Helpers {

    // NOTE: HttpServiceCallRequest provides just a tiny bit of extra functionality
    //       to the plain ServiceCallRequest by adding the ability to get the payload
    //       from an object.  This is useful when making a call from code which is 
    //       external to XerviceKit by allowing the caller to specify an object instead of 
    //       a json string for Payload.
    public class HttpServiceCallRequest : ServiceCallRequest {
        public bool UsesPayloadObj { get; private set; }

        public dynamic PayloadObj {
            set {
                Payload = Json.ToJson(value);
                UsesPayloadObj = true;
            }
        }
    }
}

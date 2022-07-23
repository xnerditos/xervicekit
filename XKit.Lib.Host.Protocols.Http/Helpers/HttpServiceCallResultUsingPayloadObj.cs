using System;
using System.Text.Json.Serialization;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Utility.Extensions;

namespace XKit.Lib.Host.Protocols.Http.Helpers {

    // NOTE: HttpServiceCallResultUsingPayloadObj changes the result slightly to use an object for PayloadObj
    //       instead of a json string for Payload.  When the caller uses PayloadObj, it is understood
    //       that the response should also use PayloadObj
    public class HttpServiceCallResultUsingPayloadObj  {
        private string payload; 

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ServiceCallStatusEnum ServiceCallStatus { get; set; } = ServiceCallStatusEnum.NotAttempted;
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LogResultStatusEnum? OperationStatus { get; set; }
        
        public ServiceInstanceStatus ServiceStatus { get; set; }
        public string Message { get; set; }
        public string ResponderInstanceId { get; set; }
        public string ResponderFabricId { get; set; }
        public Guid OperationId { get; set; }
        public string OperationName { get; set; }
        public Descriptor Service { get; set; }
        public string CorrelationId { get; set; }
        public string RequestorInstanceId { get; set; }
        public string RequestorFabricId { get; set; }
        public object Code { get; set; }
        public DateTime Timestamp { get; set; }

        public static HttpServiceCallResultUsingPayloadObj CreateFrom(ServiceCallResult from) {
            return new HttpServiceCallResultUsingPayloadObj {
                Code = from.Code,
                CorrelationId = from.CorrelationId,
                Message = from.Message,
                OperationId = from.OperationId,
                OperationName = from.OperationName,
                OperationStatus = from.OperationStatus,
                Payload = from.Payload,
                RequestorFabricId = from.RequestorFabricId,
                RequestorInstanceId = from.RequestorInstanceId,
                ResponderFabricId = from.ResponderFabricId,
                ResponderInstanceId = from.ResponderInstanceId,
                Service = from.Service,
                ServiceCallStatus = from.ServiceCallStatus,
                ServiceStatus = from.ServiceStatus,
                Timestamp = from.Timestamp
            };
        }

        public string Payload {
            set => payload = value;
        }

        public dynamic PayloadObj {
            get {
                if (payload != null) {
                    return Json.FromJson(payload);
                }
                return null;
            }
        }
    }
}

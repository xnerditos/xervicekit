using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Log;
using System;
using XKit.Lib.Common.Utility.Extensions;
using System.Text.Json.Serialization;

namespace XKit.Lib.Common.Fabric {

    public class ServiceCallResult {

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

        [JsonIgnore]
        public bool HasError => 
            !(this.OperationStatus == LogResultStatusEnum.Success || 
            this.OperationStatus == LogResultStatusEnum.PartialSuccess ||
            this.OperationStatus == LogResultStatusEnum.Pending);

        [JsonIgnore]
        public bool ImmediateSuccess => 
            this.OperationStatus == LogResultStatusEnum.Success || 
            this.OperationStatus == LogResultStatusEnum.PartialSuccess;

        [JsonIgnore]
        public bool Completed => this.ServiceCallStatus == ServiceCallStatusEnum.Completed;

        public string Payload { get; set; }

        public T GetBody<T>() where T : class {
            return Json.From<T>(this.Payload);
        }

        public object GetBody(System.Type type) {
            return Json.From(this.Payload, type);
        }

        public object GetBody() {
            return Json.FromDynamic(this.Payload);
        }

        public void SetBody(object body) {
            this.Payload = Json.ToDynamic(body);
        }

        public void SetBody<T>(T body) {
            this.Payload = Json.To<T>(body);
        }

        public ServiceCallResult<T> ConvertTo<T>() where T : class
            => new ServiceCallResult<T> {
                Code = this.Code,
                CorrelationId = this.CorrelationId,
                Message = this.Message,
                OperationId = this.OperationId,
                OperationName = this.OperationName,
                OperationStatus = this.OperationStatus,
                Payload = this.Payload,
                RequestorFabricId = this.RequestorFabricId,
                RequestorInstanceId = this.RequestorInstanceId,
                ResponderFabricId = this.ResponderFabricId,
                ResponderInstanceId = this.ResponderInstanceId,
                Service = this.Service,
                ServiceCallStatus = this.ServiceCallStatus,
                ServiceStatus = this.ServiceStatus,
                Timestamp = this.Timestamp
            };
    }

    public class ServiceCallResult<T> : ServiceCallResult where T : class { 

        private T responseBody = null; 

        public T ResponseBody { 
            get {
                if (responseBody != null) { return responseBody; }
                if (this.Payload == null || this.Payload.Length == 0) { return null; }
                responseBody = GetBody<T>();
                return responseBody;
            } 
            set {
                responseBody = value;
                SetBody(value);
            }
        } 
    }
}

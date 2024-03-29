using XKit.Lib.Common.Utility.Extensions;

namespace XKit.Lib.Common.Fabric {

    public class ServiceCallRequest {

        private string payload; 

        public ServiceCallTypeParameters CallTypeParameters { get; set; }
        public string OperationName { get; set; }
        public string RequestorInstanceId { get; set; }
        public string RequestorFabricId { get; set; }
        public string CorrelationId { get; set; }
        public string Payload { 
            get => payload; 
            set {
                if (value != null) {
                    payload = value;
                }
            } 
        }

        public T GetBody<T>() where T : class {
            return Json.FromJson<T>(this.Payload);
        }

        public object GetBody(System.Type type) {
            return Json.FromJson(this.Payload, type);
        }

        public dynamic GetBody() {
            return Json.FromJson(this.Payload);
        }

        public void SetBody(object body) {
            payload = Json.ToJson(body);
        }

        public void SetBody<T>(T body) {
            payload = Json.ToJson<T>(body);
        }

        public ServiceCallRequest() { }

        protected ServiceCallRequest(
            ServiceCallTypeParameters callTypeParameters,
            string payload,
            string correlationId,
            string operationName,
            string requestorFabricId,
            string requestorInstanceId
        ) {
            this.OperationName = operationName;
            this.CallTypeParameters = callTypeParameters ?? ServiceCallTypeParameters.SyncResult();;
            this.Payload = payload;
            this.RequestorFabricId = requestorFabricId ?? string.Empty;
            this.RequestorInstanceId = requestorInstanceId ?? string.Empty;
            this.CorrelationId = correlationId ?? Utility.Identifiers.GenerateIdentifier();
        }

        public ServiceCallRequest<T> ConvertTo<T>() where T : class
            => new() {
                CallTypeParameters = this.CallTypeParameters,
                CorrelationId = this.CorrelationId,
                OperationName = this.OperationName,
                Payload = this.Payload,
                RequestorFabricId = this.RequestorFabricId,
                RequestorInstanceId = this.RequestorInstanceId
            };
        
        public ServiceCallRequest Clone()             
            => new() {
                CallTypeParameters = this.CallTypeParameters,
                CorrelationId = this.CorrelationId,
                OperationName = this.OperationName,
                Payload = this.Payload,
                RequestorFabricId = this.RequestorFabricId,
                RequestorInstanceId = this.RequestorInstanceId
            };

        public static ServiceCallRequest Create(
            string operationName,
            string payload,
            string requestorFabricId,
            string correlationId,
            string requestorInstanceId,
            ServiceCallTypeParameters callTypeParameters = null
        ) => new(
            callTypeParameters,
            payload,
            correlationId,
            operationName,
            requestorFabricId,
            requestorInstanceId
        );
    }

    public class ServiceCallRequest<T> : ServiceCallRequest where T : class {

        private T requestBody = null; 

        public T RequestBody { 
            get {
                if (requestBody != null) { return requestBody; }
                if (this.Payload == null || this.Payload.Length > 4) { return null; }
                requestBody = GetBody<T>();
                return requestBody;
            } 
            set {
                requestBody = value;
                SetBody(value);
            }
        } 

        public new ServiceCallRequest<T> Clone()             
            => new() {
                CallTypeParameters = this.CallTypeParameters,
                CorrelationId = this.CorrelationId,
                OperationName = this.OperationName,
                Payload = this.Payload,
                RequestorFabricId = this.RequestorFabricId,
                RequestorInstanceId = this.RequestorInstanceId
            };

        public ServiceCallRequest() {}

        protected ServiceCallRequest(
            string correlationId,
            string operationName,
            ServiceCallTypeParameters callTypeParameters,
            string requestorFabricId,
            string requestorInstanceId,
            T requestBody
        ) : base(
            callTypeParameters,
            null,
            correlationId,
            operationName, 
            requestorFabricId, 
            requestorInstanceId
        ) { 
            this.RequestBody = requestBody;
        }

        public static ServiceCallRequest<T> Create(
            string operationName,
            T requestBody,
            string requestorFabricId,
            string correlationId,
            string requestorInstanceId,
            ServiceCallTypeParameters callTypeParameters = null
        ) => new(
            correlationId,
            operationName,
            callTypeParameters,
            requestorFabricId,
            requestorInstanceId,
            requestBody
        );
    }
}

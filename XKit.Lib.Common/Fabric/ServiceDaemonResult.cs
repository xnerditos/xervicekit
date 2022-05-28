using XKit.Lib.Common.Registration;
using System.Text.Json.Serialization;
using XKit.Lib.Common.Log;

namespace XKit.Lib.Common.Fabric {

    public class ServiceDaemonResultBase {

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LogResultStatusEnum? OperationStatus { get; set; }
        
        public ServiceInstanceStatus ServiceStatus { get; set; }
        public string Message { get; set; }
        public string OperationId { get; set; }
        public string OperationName { get; set; }
        public Descriptor Service { get; set; }
        public string CorrelationId { get; set; }
        public object Code { get; set; }

        [JsonIgnore]
        public bool HasError => 
            !(this.OperationStatus == LogResultStatusEnum.Success || 
            this.OperationStatus == LogResultStatusEnum.PartialSuccess ||
            this.OperationStatus == LogResultStatusEnum.Pending);

        [JsonIgnore]
        public bool ImmediateSuccess => 
            this.OperationStatus == LogResultStatusEnum.Success || 
            this.OperationStatus == LogResultStatusEnum.PartialSuccess;

        public virtual object GetBody() => null;
    }

    public class ServiceDaemonResult<T> : ServiceDaemonResultBase where T : class { 

        public T ResponseBody { get; set; }

        public override object GetBody() => ResponseBody;
    }

    public class ServiceDaemonResult : ServiceDaemonResult<object> {}

}

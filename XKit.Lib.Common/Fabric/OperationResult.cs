using System.Text.Json.Serialization;

using XKit.Lib.Common.Log;

namespace XKit.Lib.Common.Fabric {

    public class OperationResult {

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LogResultStatusEnum? OperationStatus { get; set; }
        public string Message { get; set; }
        public string LogMessage { get; set; }
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

        [JsonIgnore]
        public bool IsPending => 
            this.OperationStatus == LogResultStatusEnum.Pending;
    }

    public class OperationResult<T> : OperationResult where T : class { 

        public T ResultData { get; set; }

        public T LogData { get; set; }
    }
}

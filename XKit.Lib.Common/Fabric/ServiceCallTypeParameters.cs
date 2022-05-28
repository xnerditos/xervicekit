using System.Text.Json.Serialization;

namespace XKit.Lib.Common.Fabric {
    
    public class ServiceCallTypeParameters {

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ServiceCallTypeEnum CallType { get; set; } = ServiceCallTypeEnum.SyncResult;
        public static ServiceCallTypeParameters SyncResult() 
            => new() { CallType = ServiceCallTypeEnum.SyncResult };
        public static ServiceCallTypeParameters FireAndForget() 
            => new() { CallType = ServiceCallTypeEnum.FireAndForget };
    }
}

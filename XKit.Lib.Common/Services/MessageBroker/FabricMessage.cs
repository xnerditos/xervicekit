using System;

namespace XKit.Lib.Common.Services.MessageBroker {

    public interface IReadOnlyFabricMessage {
        Guid MessageId { get; }
        string MessageTypeName { get; }
        string JsonPayload { get; }
        string OriginatorRequestorFabricId { get; }
        string OriginatorRequestorInstanceId { get; }
        string OriginatorCorrelationId { get; }
    }

    public class FabricMessage : IReadOnlyFabricMessage {
        public Guid MessageId { get; set; }
        public string MessageTypeName { get; set; }
        public string JsonPayload { get; set; }
        public string OriginatorRequestorFabricId { get; set; }
        public string OriginatorRequestorInstanceId { get; set; }
        public string OriginatorCorrelationId { get; set; }
    }
}
namespace Samples.MessageBroker; 
public class MessageEngineParameters {
    public int[] DefaultDeliveryFailureDelaysToRetryMs { get; set; }
    public int DefaultMaxItemRetries { get; set; }
    public int DefaultMaxConsecutiveFailuresPerQueue { get; set; }
}

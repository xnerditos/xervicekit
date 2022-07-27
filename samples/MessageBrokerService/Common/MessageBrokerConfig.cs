namespace Samples.MessageBroker; 
public class MessageBrokerConfig {

    private static readonly int[] DeliveryFailureDelaysToRetryMsDefault = new int[] {
        // seconds
        1   * 1000,
        5   * 1000,
        10  * 1000,
        // minutes
        1   * 60 * 1000,
        1   * 60 * 1000,
        1   * 60 * 1000,
        1   * 60 * 1000,
        1   * 60 * 1000,
        // hours
        // 1   * 3600 * 1000,
        // 1   * 3600 * 1000,
        // 1   * 3600 * 1000,
        // 1   * 3600 * 1000,
        // 12   * 3600 * 1000
    };

    public class WaitOnMessageType {
        /// <summary>
        /// How long to wait by default in a call to WaitOnMessage
        /// </summary>
        public int? DefaultTimeoutMs { get; set; } = 20 * 1000;
        /// <summary>
        /// How long to sleep between checks in a call to WaitOnMessage
        /// </summary>
        public int? SleepWhileWaitingMs { get; set; } = 1000;
    }

    public class DeliveryDaemonType { 

        public int? MaxConcurrentMessages { get; set; } = 10;
        public int[] DefaultDeliveryFailureDelaysToRetryMs { get; set; } = DeliveryFailureDelaysToRetryMsDefault;
        
        // NOTE:  The defaults are set up so that the queue will go offline before messages get killed
        public int? DefaultMaxItemRetries { get; set; } = 7;
        public int? DefaultMaxConsecutiveFailuresPerQueue { get; set; } = 6;

        // These settings control how often the daemon checks for waiting items when it thinks there are none.
        // In theory this should not happen and would be very edge case. 
        public bool? SanityCheckEnabled { get; set; } = true;
        public int? SanityCheckDelayMs { get; set; } = 20 * 1000;      // 20 seconds
    }

    public class HousekeepingDaemonType { 
        public bool? Enable { get; set; } = true;
        public uint? CheckEveryXSeconds { get; set; } = 60 * 60 * 4; // 4 hours
        public uint? DeleteCompletedItemsOlderThanXMinutes { get; set; } = 60 * 24 * 15; // 15 days
        public uint? DeleteDeadItemsOlderThanXMinutes { get; set; } = 60 * 24 * 60; // 60 days
    }

    public HousekeepingDaemonType HousekeepingDaemon { get; set; } = new HousekeepingDaemonType();

    public WaitOnMessageType WaitOnMessage { get; set; } = new WaitOnMessageType();

    public DeliveryDaemonType DeliveryDaemon { get; set; } = new DeliveryDaemonType();
}

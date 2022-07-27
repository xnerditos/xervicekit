using XKit.Lib.Common.Services.MessageBroker;
using XKit.Lib.Common.Utility;

namespace Samples.MessageBroker.Engine;
public static class DataExtensions {

    public static string GetQueueName(this IReadOnlySubscription obj) 
        => $"{Identifiers.GetServiceVersionLevelKey(obj.Recipient)}:{obj.RecipientHostId}:{obj.MessageTypeName}";
    public static string GetQueueName(this Subscription obj) 
        => $"{Identifiers.GetServiceVersionLevelKey(obj.Recipient)}:{obj.RecipientHostId}:{obj.MessageTypeName}";
}

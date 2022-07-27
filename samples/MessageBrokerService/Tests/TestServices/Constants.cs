using XKit.Lib.Common.Registration;

namespace Samples.MessageBroker.Tests.TestServices;
 
public static class Constants {
    public const string CollectionName = "TestRecipients";
    public static readonly IReadOnlyDescriptor Service1 = new Descriptor {
        Collection = CollectionName,
        Name = "Service1",
        Version = 1,
        UpdateLevel = 0,
        PatchLevel = 0
    };
    public static readonly IReadOnlyDescriptor Service2 = new Descriptor {
        Collection = CollectionName,
        Name = "Service2",
        Version = 1,
        UpdateLevel = 0,
        PatchLevel = 0
    };
    public static readonly IReadOnlyDescriptor Service3 = new Descriptor {
        Collection = CollectionName,
        Name = "Service3",
        Version = 1,
        UpdateLevel = 0,
        PatchLevel = 0
    };
    public static readonly IReadOnlyDescriptor Service4 = new Descriptor {
        Collection = CollectionName,
        Name = "Service4",
        Version = 1,
        UpdateLevel = 0,
        PatchLevel = 0
    };
}

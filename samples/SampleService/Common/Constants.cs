using XKit.Lib.Common.Registration;

namespace Samples.SampleService.V1; 

public static class Constants {
    public const string CollectionName = "Samples"; 
    public const string ServiceName = "SampleService";
    public const int Version = 1;
    public const int UpdateLevel = 0;
    public const int PatchLevel = 0;

    public static readonly IReadOnlyDescriptor ServiceDescriptor = new Descriptor {
        Collection = CollectionName,
        Name = ServiceName,
        Version = Version,
        UpdateLevel = UpdateLevel,
        PatchLevel = PatchLevel
    };
}

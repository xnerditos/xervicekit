using XKit.Lib.Common.Registration;

namespace Tutorial.User;

public static class Constants
{
    public static readonly Descriptor ServiceDescriptor = new Descriptor {
        Collection = "Tutorial",
        Name = "User",
        Version = 1
    };
}

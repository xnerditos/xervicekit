using XKit.Lib.Common.Registration;

namespace Tutorial.Session
{
    public static class Constants
    {
        public static readonly Descriptor ServiceDescriptor = new() {
            Collection = "Tutorial",
            Name = "Session",
            Version = 1
        };
    }
}

using XKit.Lib.Common.Registration;

namespace XKit.Lib.Common.Services {

    public interface IServiceFactory {

        IReadOnlyDescriptor Descriptor { get; }
    }
}
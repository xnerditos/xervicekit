using XKit.Lib.Common.Host;
using XKit.Lib.Common.Services;

namespace TestServices; 

public interface ITestServiceFactory : IServiceFactory {

    IManagedService Create(
        IXkitHostEnvironment hostEnv
    );
}

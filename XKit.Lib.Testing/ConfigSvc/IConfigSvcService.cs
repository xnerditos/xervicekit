using XKit.Lib.Common.Host;

namespace XKit.Lib.Testing.TestConfigSvc {
    public interface IConfigSvcService : IManagedService, IServiceBase {

        string GetConfigJsonForService(string serviceIdentifier);
        HostConfigDocument GetConfigForHost();
    }
}

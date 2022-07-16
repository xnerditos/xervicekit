using XKit.Lib.Common.Host;
using XKit.Lib.Host.DefaultBaseClasses;

namespace XKit.Lib.Host.Services {

    public interface INoOpServiceOperationApi : IServiceOperation, IServiceApi { }

    public class NoOpServiceOperation : ServiceOperation, INoOpServiceOperationApi {

        public NoOpServiceOperation(
            ServiceOperationContext context
        ) : base(
            context
        ) { }
    }
}

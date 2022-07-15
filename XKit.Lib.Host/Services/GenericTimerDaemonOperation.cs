using XKit.Lib.Common.Host;
using XKit.Lib.Host.DefaultBaseClasses;

namespace XKit.Lib.Host.Services {

    public class GenericTimerDaemonOperation : ServiceDaemonOperation<object>, IGenericTimerDaemonOperation {

        public GenericTimerDaemonOperation(
            ServiceDaemonOperationContext context
        ) : base(
            context
        ) { }
    }
}

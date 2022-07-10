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

    public interface INoOpDaemonTimerOperationApi : IServiceDaemonOperation { }

    public class NoOpDaemonTimerOperation : ServiceDaemonOperation, INoOpDaemonTimerOperationApi {

        public NoOpDaemonTimerOperation(
            ServiceDaemonOperationContext context
        ) : base(
            context
        ) { }

    }

    public interface INoOpDaemonMessageOperationApi : IServiceDaemonOperation<object> { }

    public class NoOpDaemonMessageOperation 
        : ServiceDaemonOperation<object>, INoOpDaemonMessageOperationApi {

        public NoOpDaemonMessageOperation(
            ServiceDaemonOperationContext context
        ) : base(
            context
        ) { }

    }

    // public interface INoOpDaemonTimerOperationApi : IServiceOperation, IServiceApi { }

    // public class NoOpDaemonTimerOperation : ServiceDaemonOperation, INoOpDaemonTimerOperationApi {

    //     public NoOpDaemonTimerOperation(
    //         ServiceDaemonOperationContext context
    //     ) : base(
    //         context
    //     ) { }

    // }
}

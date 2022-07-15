using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;

namespace XKit.Lib.Common.Host {

    /// <summary>
    /// Classes that implement the concrete operations of a service daemon operation
    /// implement this interface
    /// </summary>
    public interface IServiceDaemonOperation : IOperation { 
        Task<OperationResult> RunDaemonTimerOperation();
    }

    /// <summary>
    /// Classes that implement the concrete operations of a service daemon operation
    /// implement this interface
    /// </summary>
    public interface IServiceDaemonOperation<TMessage> : IServiceDaemonOperation
        where TMessage : class { 

        Task<OperationResult> RunDaemonMessageOperation(TMessage message) 
            => Task.FromResult(new OperationResult { OperationStatus = Log.LogResultStatusEnum.Unknown });
    }
}

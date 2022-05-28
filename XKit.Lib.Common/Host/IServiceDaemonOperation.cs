using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;

namespace XKit.Lib.Common.Host {

    /// <summary>
    /// Classes that implement the concrete operations of a service daemon operation
    /// implement this interface
    /// </summary>
    public interface IServiceDaemonOperation : IOperation { }

    /// <summary>
    /// Classes that implement the concrete operations of a service daemon operation
    /// implement this interface
    /// </summary>
    public interface IServiceDaemonOperation<TMessage> : IServiceDaemonOperation
        where TMessage : class { 

        Task<OperationResult> RunDaemonOperation(TMessage message);
    }
}
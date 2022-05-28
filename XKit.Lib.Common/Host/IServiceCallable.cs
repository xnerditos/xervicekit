
namespace XKit.Lib.Common.Host {


    public interface IServiceCallable { }

    /// <summary>
    /// Interfaces that describe a service api must derive from this interface.
    /// </summary>
    public interface IServiceApi : IServiceCallable { }

    /// <summary>
    /// Base interface for things that can be subscribed to
    /// </summary>
    public interface IServiceSubscribable : IServiceCallable { }

    /// <summary>
    /// Interfaces that fabric command handlers should derive from this interface.
    /// </summary>
    public interface IServiceCommands : IServiceSubscribable { }

    /// <summary>
    /// Interfaces that fabric event handlers should derive from this interface.
    /// </summary>
    public interface IServiceEvents : IServiceSubscribable { }
}
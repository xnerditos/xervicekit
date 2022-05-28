using System;

namespace XKit.Lib.Common.Host {

    /// <summary>
    /// Classes that implement the concrete operations of an operation
    /// implement this interface
    /// </summary>
    public interface IOperation { 
        bool IsActive { get; }
    }
}
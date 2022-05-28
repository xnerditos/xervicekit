using System;
using Moq;
using XKit.Lib.Common.Host;

namespace XKit.Lib.Testing.Mocking {

    public interface IMockService<TApiInterface>
        where TApiInterface : class, IServiceCallable { 

        Mock<TApiInterface> ApiMock { get; }
    }
}
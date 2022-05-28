
using Moq;
using XKit.Lib.Common.Host;

namespace XKit.Lib.Testing.Mocking {

    public interface IMockOperation : IServiceOperation  {}

    public interface IMockOperationMoqApi<TApiInterface> : IMockOperation 
        where TApiInterface : class { 
        Mock<TApiInterface> ApiMock { get; }
    }
}
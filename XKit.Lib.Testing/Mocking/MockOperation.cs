using Moq;
using XKit.Lib.Common.Host;
using XKit.Lib.Host.DefaultBaseClasses;

namespace XKit.Lib.Testing.Mocking {

    public partial class MockOperation<TCallInterface> 
        : ServiceOperation, IMockOperationMoqApi<TCallInterface>
        where TCallInterface : class, IServiceCallable {

        private readonly Mock<TCallInterface> apiMock;

        public MockOperation(
            ServiceOperationContext context,
            Mock<TCallInterface> apiMock
        ) : base(
            context
        ) { 
            this.apiMock = apiMock;
        }

        Mock<TCallInterface> IMockOperationMoqApi<TCallInterface>.ApiMock => apiMock;
    }
}
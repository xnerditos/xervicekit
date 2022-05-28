using Moq;

namespace UnitTests.MockWrapper {

    public abstract class MockWrapperBase {

        protected MockWrapperManager MockWrappersManager { get; private set; }

        public abstract void VerifyMock();

        internal void SetMocksManager(MockWrapperManager manager) {
            this.MockWrappersManager = manager;
        }
    }

    public class MockWrapperBase<T> : MockWrapperBase where T : class {

        private readonly Mock<T> mock;

        public Mock<T> Mock => mock;
        public T Object => Mock.Object;

        public MockWrapperBase() : this(new Mock<T>(MockBehavior.Strict)) {}
        
        public MockWrapperBase(Mock<T> mock) {
            this.mock = mock;
        }

        public override void VerifyMock() {
            Mock.VerifyAll();
        }
    }
}
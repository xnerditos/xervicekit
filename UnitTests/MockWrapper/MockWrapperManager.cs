using System.Collections.Generic;
using Moq;

namespace UnitTests.MockWrapper {

    public class MockWrapperManager {
        private readonly List<MockWrapperBase> Wrappers = new List<MockWrapperBase>();

        public TMockWrapper CreateWrapper<TMockWrapper>() where TMockWrapper : MockWrapperBase, new() {
            var wrapper = new TMockWrapper();
            wrapper.SetMocksManager(this);
            Wrappers.Add(wrapper);
            return wrapper;
        }

        public void VerifyAll() {
            foreach(var wrapper in Wrappers) {
                wrapper.VerifyMock();
            }
        }        
    }
}
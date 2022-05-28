using System;
using UnitTests.MockWrapper;

namespace UnitTests {

    public class TestBase {

        protected MockWrapperManager Mocks = new MockWrapperManager();

        // =====================================================================
        // protected create
        // =====================================================================
        protected string CreateRandomString()
            => Guid.NewGuid().ToString();
    }
}
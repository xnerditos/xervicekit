using System.Linq;
using XKit.Lib.Common.Host;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Host.XKitHostAssertions.XKitHost {

    [TestClass]
    public class RegisterManagedService : XKitHostTestsCommon {
                
        [TestMethod]
        public void SuccessfullyRegisters() {
            
            var target = CreateTarget();
            var service = new TestManagedService();
            
            target.AddManagedService(service);

            var service2 = target.GetManagedServices().Single();
            service2.Should().NotBeNull();
        }
    }
}

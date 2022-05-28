using XKit.Lib.Common.Host;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Host.HostManagerAssertions.HostManager {

    [TestClass]
    public class RegisterMetaService : HostManagerTestsCommon {
                
        [TestMethod]
        public void SuccessfullyRegisters() {
            
            var target = CreateTarget();
            var service = new TestMetaService();
            
            target.AddMetaService(service);

            target.GetMetaServices(service.Descriptor.Name).Should().NotBeNull();
        }
    }
}
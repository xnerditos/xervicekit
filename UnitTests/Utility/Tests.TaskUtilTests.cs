using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XKit.Lib.Common.Utility.Threading;

namespace UnitTests.UtilityAssertions {

    [TestClass]
    public class TaskUtilTests {

        [TestMethod]
        public void TaskReturningMethodSafelyPassesException() {

            try {
                TaskUtil.RunAsyncAsSync(() => TaskReturningMethodThrowsException());
            } catch (Exception ex) {
                // good!

                ex.Message.Should().Be("some message");
                return;
            }
            Assert.Fail("Should have gone into exception block above");
        }

        [TestMethod]
        public void AsyncMethodSafelyPassesException() {

            try {
                TaskUtil.RunAsyncAsSync(() => AsyncMethodThrowsException());
            } catch (Exception ex) {
                // good!

                ex.Message.Should().Be("Could not find file '/somefilethatdoesnotexist'.");
                return;
            }
            Assert.Fail("Should have gone into exception block above");
        }

        private static Task TaskReturningMethodThrowsException() {
            try {
                throw new Exception("some message");
            } catch (Exception ex) {
                return Task.FromException(ex);
            }
        }

        private static async Task AsyncMethodThrowsException() {
            await File.ReadAllTextAsync("/somefilethatdoesnotexist");
        }
    }
}

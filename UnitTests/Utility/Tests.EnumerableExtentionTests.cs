using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XKit.Lib.Common.Utility.Extensions;

namespace UnitTests.UtilityAssertions {

    [TestClass]
    public class EnumerableExtentionTests {

        [TestMethod]
        public void Split_Succeeds() {
            
            var source = new[] { 
                "4",
                "5",
                "0",
                "1",
                "3"
            };

            var split = source.Split(s => int.Parse(s));

            split.Should().BeEquivalentTo(new[] { 
                new string[] { "0" },
                new string[] { "1" },
                new string[0],
                new string[] { "3" },
                new string[] { "4" },
                new string[] { "5" }
            });
        }
    }
}
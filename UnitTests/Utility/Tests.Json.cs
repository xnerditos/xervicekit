using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XKit.Lib.Common.Utility.Extensions;
using XKit.Lib.Common.Utility.Threading;

namespace UnitTests.UtilityAssertions;

public class TestEntity {
    public int SomeInt { get; set; }
    public string SomeString { get; set; }
    public string[] SomeArrayStrings { get; set; }
    public TestEntity2[] SomeArrayObjects { get; set; }
}

public class TestEntity2 {
    public int SomeOtherInt { get; set; }
    public string SomeOtherString { get; set; }
}

[TestClass]
public class JsonTests {

    private readonly TestEntity MyTestEntity = new TestEntity { 
        SomeInt = 1,
        SomeString = "string",
        SomeArrayStrings = new[] {
            "a",
            "b"
        },
        SomeArrayObjects = new[] {
            new TestEntity2 {
                SomeOtherInt = 2,
                SomeOtherString = "another one"
            }
        }
    };

    [TestMethod]
    public void CanSerializeWithConcreteTypeBackToType() {
        var json = MyTestEntity.ToJson();
        var e2 = json.FromJson<TestEntity>();
        e2.Should().BeEquivalentTo(MyTestEntity);
    }

    [TestMethod]
    public void CanSerializeWithoutConcreteTypeBackToType() {
        var json = ((object) MyTestEntity).ToJson();
        var e2 = json.FromJson<TestEntity>();
        e2.Should().BeEquivalentTo(MyTestEntity);
    }
}

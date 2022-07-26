using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XKit.Lib.Testing;

namespace Tutorial.User.Test;

[TestClass]
public class GetUserTests
{
    private static readonly TestHostHelper testHelper = new();

    [ClassInitialize]
    public static void ClassInit(TestContext context) {
        testHelper.InitializeLocalTestHost();
        testHelper.AddCreateService(
            Constants.ServiceDescriptor,
            typeof(ApiOperation)
        );
        testHelper.StartHost();
    }

    [ClassCleanup]
    public static void ClassTeardown() {
        testHelper.DestroyHost();
    }

    [TestMethod]
    public async Task GetExistingUserSucceeds()
    {
        var client = new Client(testHelper.HostEnvironment.Connector);

        await client.DeleteUser(new User {Username = "kermit"}); // Clean up any previous data

        var upsertResult = await client.UpsertUser(new User { 
            Username = "kermit",
            Email = "kermit@thefrog.com",
            FullName = "Kermit TheFrog"
        });

        Assert.IsFalse(upsertResult.HasError);

        var getResult = await client.GetUser(new User {
            Username = "kermit"
        });

        Assert.IsFalse(getResult.HasError);
        Assert.AreEqual(getResult.ResponseBody.Username, "kermit");
        Assert.AreEqual(getResult.ResponseBody.Email, "kermit@thefrog.com");
        Assert.AreEqual(getResult.ResponseBody.FullName, "Kermit TheFrog");
    }

    [TestMethod]
    public async Task GetNonExistantUserFails()
    {
        var client = new Client(testHelper.HostEnvironment.Connector);

        var getResult = await client.GetUser(new User {
            Username = "some_user_who_does_not_exist"
        });

        Assert.IsTrue(getResult.HasError);
    }
}

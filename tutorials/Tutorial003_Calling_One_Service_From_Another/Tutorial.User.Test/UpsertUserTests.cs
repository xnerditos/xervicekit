using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XKit.Lib.Testing;

namespace Tutorial.User.Test;

[TestClass]
public class UpsertUserTests
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
    public async Task UpsertWithNewUserSucceeds()
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

        Assert.AreEqual(getResult.ResponseBody.Username, "kermit");
        Assert.AreEqual(getResult.ResponseBody.Email, "kermit@thefrog.com");
        Assert.AreEqual(getResult.ResponseBody.FullName, "Kermit TheFrog");
    }

    [TestMethod]
    public async Task UpsertWithExistingUserSucceeds()
    {
        var client = new Client(testHelper.HostEnvironment.Connector);

        await client.DeleteUser(new User {Username = "kermit"}); // Clean up any previous data

        await client.UpsertUser(new User { 
            Username = "kermit",
            Email = "kermit@thefrog.com",
            FullName = "Kermit TheFrog"
        });

        var upsertResult2 = await client.UpsertUser(new User { 
            Username = "kermit",
            Email = "kermie@frogger.com",
            FullName = "Kermit D Frog"
        });
        Assert.IsFalse(upsertResult2.HasError);

        var getResult = await client.GetUser(new User {
            Username = "kermit"
        });

        Assert.AreEqual(getResult.ResponseBody.Username, "kermit");
        Assert.AreEqual(getResult.ResponseBody.Email, "kermie@frogger.com");
        Assert.AreEqual(getResult.ResponseBody.FullName, "Kermit D Frog");
    }
}

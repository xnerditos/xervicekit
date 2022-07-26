using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XKit.Lib.Testing;

namespace Tutorial.User.Test;

[TestClass]
public class DeleteUserTests
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
    public async Task DeleteUserSucceeds()
    {
        var client = new Client(testHelper.HostEnvironment.Connector);

        var upsertResult = await client.UpsertUser(new User { 
            Username = "kermit",
            Email = "kermit@thefrog.com",
            FullName = "Kermit TheFrog"
        });

        Assert.IsFalse(upsertResult.HasError);

        var deleteResult = await client.DeleteUser(new User {
            Username = "kermit"
        });

        Assert.IsFalse(deleteResult.HasError); // Note that DeleteUser will succeed even
                                               // if the user does not exist.

        // the user doesn't exist any more, so this should fail
        var getResult = await client.GetUser(new User {
            Username = "kermit"
        });

        Assert.IsTrue(getResult.HasError);
    }
}

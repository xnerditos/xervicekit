using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XKit.Lib.Testing;
using Moq;
using Tutorial.User;
using XKit.Lib.Common.Fabric;

namespace Tutorial.Session.Test;

[TestClass]
public class IsUserLoggedInTests
{
    private static readonly TestHostHelper testHelper = new();

    [ClassInitialize]
    public static void ClassInit(TestContext context) {
        testHelper.InitializeLocalTestHost();
        testHelper.AddCreateService(
            Constants.ServiceDescriptor,
            typeof(ApiOperation)
        );
        var userMock = testHelper.AddMockService<IUserApi>(User.Constants.ServiceDescriptor);
        userMock.ApiMock
            .Setup(x => x.GetUser(It.IsAny<Tutorial.User.User>()))
            .ReturnsAsync(new ServiceCallResult<Tutorial.User.User> { 
                OperationStatus = XKit.Lib.Common.Log.LogResultStatusEnum.Success,
                ServiceCallStatus = ServiceCallStatusEnum.Completed,
                ResponseBody = new Tutorial.User.User {
                    Password = "ribbot!"
                }
            });
        testHelper.StartHost();
    }

    [ClassCleanup]
    public static void ClassTeardown() {
        testHelper.DestroyHost();
    }

    [TestMethod]
    public async Task ReturnsCorrectResponseWhenUserLoggedIn()
    {
        var client = new Client(testHelper.HostEnvironment.Connector);

        var loginResult = await client.Login(new() { 
            Username = "kermit",
            Password = "ribbot!"
        });

        var isUserLoggedInResult = await client.IsUserLoggedIn(new() { Username = "kermit" });
        Assert.IsTrue(isUserLoggedInResult.ResponseBody.UserLoggedIn);
    }

    [TestMethod]
    public async Task ReturnsCorrectResponseWhenUserLoggedOut()
    {
        var client = new Client(testHelper.HostEnvironment.Connector);

        await client.Login(new() { 
            Username = "kermit",
            Password = "ribbot!"
        });
        await client.Logout(new() { Username = "kermit" });
        var isUserLoggedInResult = await client.IsUserLoggedIn(new() { Username = "kermit" });
        Assert.IsFalse(isUserLoggedInResult.ResponseBody.UserLoggedIn);
    }

    [TestMethod]
    public async Task ReturnsCorrectResponseWhenUserDoesNotExist()
    {
        var client = new Client(testHelper.HostEnvironment.Connector);

        var isUserLoggedInResult = await client.IsUserLoggedIn(new() { Username = "doesnotexist" });
        Assert.IsFalse(isUserLoggedInResult.ResponseBody.UserLoggedIn);
    }
}

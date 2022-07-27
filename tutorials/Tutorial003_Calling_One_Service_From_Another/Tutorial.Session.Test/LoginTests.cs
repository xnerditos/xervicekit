using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using XKit.Lib.Testing;
using Tutorial.User;
using XKit.Lib.Common.Fabric;

namespace Tutorial.Session.Test;

[TestClass]
public class LoginTests
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
    public async Task LoginSucceedsWithCorrectPassword()
    {
        var client = new Client(testHelper.HostEnvironment.Connector);

        var result = await client.Login(new() { 
            Username = "kermit",
            Password = "ribbot!"
        });

        Assert.IsFalse(result.HasError);
        Assert.IsTrue(result.ResponseBody.UserLoggedIn);
    }

    [TestMethod]
    public async Task LoginSucceedsWithIncorrectPassword()
    {
        var client = new Client(testHelper.HostEnvironment.Connector);

        var result = await client.Login(new() { 
            Username = "kermit",
            Password = "a bad password"
        });

        Assert.IsFalse(result.HasError);
        Assert.IsFalse(result.ResponseBody.UserLoggedIn);
    }
}

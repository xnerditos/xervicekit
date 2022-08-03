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
        testHelper.AddService(new SessionService(testHelper.HostEnvironment));
        // testHelper.AddCreateService(
        //     User.Constants.ServiceDescriptor,
        //     typeof(MockUserOperation)
        // );
        var userMock = testHelper.AddMockService<IUserApi>(User.Constants.ServiceDescriptor);
        // success case
        userMock.ApiMock
            .Setup(x => x.GetUser(It.Is<Tutorial.User.User>(x => x.Username == "alice")))
            .ReturnsAsync(new ServiceCallResult<Tutorial.User.User> { 
                OperationStatus = XKit.Lib.Common.Log.LogResultStatusEnum.Success,
                ServiceCallStatus = ServiceCallStatusEnum.Completed,
                ResponseBody = new Tutorial.User.User {
                    Email = "alice@tutorial.com",
                    Username = "alice",
                    FullName = "Alice TheUser",
                    Password = "pwd"
                }
            });
        // failure case
        userMock.ApiMock
            .Setup(x => x.GetUser(It.Is<Tutorial.User.User>(x => x.Username != "alice")))
            .ReturnsAsync(new ServiceCallResult<Tutorial.User.User> { 
                OperationStatus = XKit.Lib.Common.Log.LogResultStatusEnum.NonRetriableError,
                Message = "User not found",
                ServiceCallStatus = ServiceCallStatusEnum.Completed,
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

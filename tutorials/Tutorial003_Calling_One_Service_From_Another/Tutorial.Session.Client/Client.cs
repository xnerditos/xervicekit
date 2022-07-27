using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Connector.Service;

namespace Tutorial.Session;

public class Client : ServiceClientBase<ISessionApi>, ISessionApi
{
    public Client(IFabricConnector connector)
        : base(Constants.ServiceDescriptor, null, connector)
    {
    }

    public Task<ServiceCallResult<LoginResult>> Login(LoginRequest request)
    {
        return ExecuteCall<LoginRequest, LoginResult>(request);
    }

    public Task<ServiceCallResult<IsUserLoggedInResult>> IsUserLoggedIn(IsUserLoggedInRequest request)
    {
        return ExecuteCall<IsUserLoggedInRequest, IsUserLoggedInResult>(request);
    }

    public Task<ServiceCallResult> Logout(LogoutRequest request)
    {
        return ExecuteCall(request);
    }
}

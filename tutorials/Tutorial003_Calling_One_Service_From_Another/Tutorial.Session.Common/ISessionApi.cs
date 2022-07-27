using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;

namespace Tutorial.Session;

public interface ISessionApi : IServiceApi
{
    Task<ServiceCallResult<LoginResult>> Login(LoginRequest request);
    Task<ServiceCallResult> Logout(LogoutRequest request);
    Task<ServiceCallResult<IsUserLoggedInResult>> IsUserLoggedIn(IsUserLoggedInRequest request);
}

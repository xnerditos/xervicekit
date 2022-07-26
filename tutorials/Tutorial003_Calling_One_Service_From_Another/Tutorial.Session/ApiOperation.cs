using System;
using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Host.DefaultBaseClasses;

namespace Tutorial.Session;

public class ApiOperation : ServiceOperation<ISessionService>, ISessionApi
{
    public ApiOperation(ServiceOperationContext context) 
        : base(context)
    {
    }

    public Task<ServiceCallResult<LoginResult>> Login(LoginRequest request) {
        return RunServiceCall(
            request,
            operationAction: DoLogin
        );
    }

    public Task<ServiceCallResult> Logout(LogoutRequest request) {
        return RunServiceCall(
            request,
            operationAction: DoLogout
        );
    }

    public Task<ServiceCallResult<IsUserLoggedInResult>> IsUserLoggedIn(IsUserLoggedInRequest request) {
        return RunServiceCall(
            request,
            operationAction: DoIsUserLoggedIn
        );
    }

    // ---------------------------------------------------------------------

    private async Task<LoginResult> DoLogin(LoginRequest request) 
    {
        var userClient = new Tutorial.User.Client(Connector);
        var getUserResult = await userClient.GetUser(new() { Username = request.Username });
        if (getUserResult.HasError) { 
            Log.Error("Can't find that user!");
            return null;
        }

        // check for bad password
        if (getUserResult.ResponseBody.Password != request.Password) {
            Log.Audit("Invalid user credentials!");
            return new() { UserLoggedIn = false };
        }

        // if we got this far, we are good!

        var sessions = Service.GetSessions();
        sessions[request.Username] = DateTime.Now;
        return new() { UserLoggedIn = true };
    }

    private Task DoLogout(LogoutRequest request) 
    {  
        var sessions = Service.GetSessions();
        sessions.TryRemove(request.Username, out var _);
        return Task.CompletedTask;
    }

    private Task<IsUserLoggedInResult> DoIsUserLoggedIn(IsUserLoggedInRequest request) 
    {   
        var sessions = Service.GetSessions();
        var isLoggedIn = sessions.ContainsKey(request.Username);
        return Task.FromResult(new IsUserLoggedInResult { UserLoggedIn = isLoggedIn });
    }
}

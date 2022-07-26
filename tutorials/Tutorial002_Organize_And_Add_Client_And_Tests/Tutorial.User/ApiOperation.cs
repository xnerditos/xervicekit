using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Host.DefaultBaseClasses;
using Newtonsoft.Json;
using System.IO;

namespace Tutorial.User;

public class ApiOperation : ServiceOperation, IUserApi
{
    public ApiOperation(ServiceOperationContext context) 
        : base(context)
    {
    }

    public Task<ServiceCallResult> UpsertUser(User request)
    {
        return RunServiceCall(
            request,
            operationAction: DoUpsertUser
        );
    }

    public Task<ServiceCallResult> DeleteUser(User request)
    {
        return RunServiceCall(
            request,
            operationAction: DoDeleteUser
        );
    }

    public Task<ServiceCallResult<User>> GetUser(User request)
    {
        return RunServiceCall(
            request,
            operationAction: DoGetUser
        );
    }

    // ---------------------------------------------------------------------

    private async Task DoUpsertUser(User request) 
    {   
        var jsonString = JsonConvert.SerializeObject(request);
        var path = $"{HostEnvironment.DataRootFolderPath}/{request.Username}.json";
        await File.WriteAllTextAsync(path, jsonString);
    }

    private Task DoDeleteUser(User request) 
    {   
        // NOTE:  A method that returns a task but does not run async should 
        //        catch any exceptions and return them using Task.FromException().
        //        However, for simplicity, we are leaving things like this for now.
        var path = $"{HostEnvironment.DataRootFolderPath}/{request.Username}.json";
        File.Delete(path);
        return Task.CompletedTask;
    }

    private async Task<User> DoGetUser(User request) 
    {   
        var path = $"{HostEnvironment.DataRootFolderPath}/{request.Username}.json";
        var jsonStringFromDisk = await File.ReadAllTextAsync(path);
        return JsonConvert.DeserializeObject<User>(jsonStringFromDisk);
    }
}

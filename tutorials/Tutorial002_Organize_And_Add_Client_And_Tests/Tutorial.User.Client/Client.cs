using System;
using System.Threading.Tasks;
using XKit.Lib.Common.Client;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Registration;
using XKit.Lib.Connector.Service;

namespace Tutorial.User;

public class Client : ServiceClientBase<IUserApi>, IUserApi
{
    public Client(IFabricConnector connector)
        : base(Constants.ServiceDescriptor, null, connector)
    {
    }

    public Task<ServiceCallResult> UpsertUser(User request)
    {
        return this.ExecuteCall(request);
    }

    public Task<ServiceCallResult<User>> GetUser(User request)
    {
        return this.ExecuteCall<User, User>(request);
    }

    public Task<ServiceCallResult> DeleteUser(User request)
    {
        return this.ExecuteCall(request);
    }
}

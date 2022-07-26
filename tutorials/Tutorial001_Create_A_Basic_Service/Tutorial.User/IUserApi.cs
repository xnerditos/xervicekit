using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;

namespace Tutorial.User;

public interface IUserApi : IServiceApi
{
    Task<ServiceCallResult> UpsertUser(User request);
    Task<ServiceCallResult> DeleteUser(User request);
    Task<ServiceCallResult<User>> GetUser(User request);
}

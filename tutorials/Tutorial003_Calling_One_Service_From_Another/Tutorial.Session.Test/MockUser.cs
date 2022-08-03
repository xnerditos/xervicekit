using System;
using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Log;
using XKit.Lib.Host.DefaultBaseClasses;

namespace Tutorial.Session.Test;

public class MockUserOperation : ServiceOperation, User.IUserApi
{
    public MockUserOperation(ServiceOperationContext context) 
        : base(context)
    {
    }

    public Task<ServiceCallResult<Tutorial.User.User>> GetUser(Tutorial.User.User request) 
    {
        return RunServiceCall(
            request,
            operationAction: (req) => {
                // success case
                if (request.Username == "alice") {
                    return Task.FromResult(new OperationResult<Tutorial.User.User> {
                        OperationStatus = LogResultStatusEnum.Success,
                        ResultData = new Tutorial.User.User {
                            Email = "alice@tutorial.com",
                            Username = "alice",
                            FullName = "Alice TheUser",
                            Password = "pwd"
                        }
                    });
                }
                // failure case
                return Task.FromResult(new OperationResult<Tutorial.User.User> {
                    OperationStatus = LogResultStatusEnum.NonRetriableError,
                    Message = "User not found"
                });
            }
        );
    }

    public Task<ServiceCallResult> UpsertUser(User.User request) {
        throw new NotImplementedException();
    }

    public Task<ServiceCallResult> DeleteUser(Tutorial.User.User request) 
    {
        throw new NotImplementedException();
    }
}

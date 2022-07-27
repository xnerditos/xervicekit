using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;
using XKit.Lib.Host.DefaultBaseClasses;

namespace Tutorial.Session;

public class SessionService : ManagedService<ApiOperation>, ISessionService
{
    private ConcurrentDictionary<string, DateTime> sessions = new();
    
    public SessionService(IXKitHostEnvironment hostEnv) 
        : base(hostEnv)
    {
    }

    protected override IReadOnlyDescriptor Descriptor => Constants.ServiceDescriptor;

    public ConcurrentDictionary<string, DateTime> GetSessions() {
        return sessions;
    }
}

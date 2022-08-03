using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

    protected override IEnumerable<IReadOnlyDescriptor> Dependencies => new[] {
        User.Constants.ServiceDescriptor
    };
    
    public ConcurrentDictionary<string, DateTime> GetSessions() {
        return sessions;
    }
}

using System;
using System.Collections.Concurrent;
using XKit.Lib.Common.Host;

namespace Tutorial.Session;

public interface ISessionService : IManagedService
{
    ConcurrentDictionary<string, DateTime> GetSessions();
}

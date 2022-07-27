using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XKit.Lib.Common.Host;
using Samples.MessageBroker.Engine;

namespace Samples.MessageBroker; 
public interface IMessageBrokerService : IManagedService {

    Task<MessageBrokerConfig> GetConfig();
    IMessageEngine GetMessageEngine();
    void SignalMessageAdded(IMessageRecord record, IReadOnlyList<Guid> queueItemIdsAdded);
}

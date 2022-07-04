using System;
using System.Collections.Generic;
using System.Linq;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;
using XKit.Lib.Log;

namespace XKit.Lib.Consumer; 

public class ConsumerXkitEnvironment : IXkitEnvironment {
    
    private readonly IFabricConnector connector; 

    private readonly Func<IEnumerable<IReadOnlyDescriptor>> getDependenciesSource;

    string IXkitEnvironment.FabricId => connector.FabricId;

    ILogSessionFactory IXkitEnvironment.LogSessionFactory => LogSessionFactory.Factory;

    IFabricConnector IXkitEnvironment.Connector => connector;

    IEnumerable<IReadOnlyDescriptor> IXkitEnvironment.GetDependencies() 
        => getDependenciesSource?.Invoke()?.Select(d => d.Clone()).ToArray() ?? Array.Empty<Descriptor>();

    public ConsumerXkitEnvironment(
        Func<IList<IReadOnlyDescriptor>> getDependenciesSource,
        IFabricConnector connector
    ) {
        this.getDependenciesSource = getDependenciesSource;
        this.connector = connector;
    }
}


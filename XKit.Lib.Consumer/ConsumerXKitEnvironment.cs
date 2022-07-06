using System;
using System.Collections.Generic;
using System.Linq;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;
using XKit.Lib.Log;

namespace XKit.Lib.Consumer; 

public class ConsumerXKitEnvironment : IXKitEnvironment {
    
    private readonly IFabricConnector connector; 

    private readonly Func<IEnumerable<IReadOnlyDescriptor>> getDependenciesSource;

    string IXKitEnvironment.FabricId => connector.FabricId;

    ILogSessionFactory IXKitEnvironment.LogSessionFactory => LogSessionFactory.Factory;

    IFabricConnector IXKitEnvironment.Connector => connector;

    IEnumerable<IReadOnlyDescriptor> IXKitEnvironment.GetDependencies() 
        => getDependenciesSource?.Invoke()?.Select(d => d.Clone()).ToArray() ?? Array.Empty<Descriptor>();

    public ConsumerXKitEnvironment(
        Func<IList<IReadOnlyDescriptor>> getDependenciesSource,
        IFabricConnector connector
    ) {
        this.getDependenciesSource = getDependenciesSource;
        this.connector = connector;
    }
}


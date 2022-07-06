using System;
using System.Collections.Generic;
using System.Linq;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common;
using XKit.Lib.Connector.Fabric;
using XKit.Lib.Common.Client;
using XKit.Lib.Common.Utility.Threading;
using System.Threading.Tasks;
using XKit.Lib.Log;
using XKit.Lib.Common.Utility;

namespace XKit.Lib.Consumer; 

public class ConsumerHelper {

    private readonly List<IReadOnlyDescriptor> Dependencies = new();
    
    private IFabricConnector connector;
    private readonly SetOnceOrThrow<ILogSession> log = new();
    
    public IFabricConnector Connector => connector;
    public string FabricId => connector.FabricId;
    public ILogSession Log => log.Value;

    public IFabricConnector CreateInitConsumer(
        IEnumerable<IServiceClientFactory> dependencies,
        IEnumerable<string> initialRegistryAddresses = null,
        IEnumerable<IInstanceClientFactory> instanceClientFactories = null,
        bool fatalIfCannotRegister = false
    ) {
        return CreateInitConsumer(
            dependencies.Select(df => df.Descriptor),
            initialRegistryAddresses,
            instanceClientFactories, 
            fatalIfCannotRegister
        );
    }

    public IFabricConnector CreateInitConsumer(
        IEnumerable<IReadOnlyDescriptor> dependencies = null,
        IEnumerable<string> initialRegistryAddresses = null,
        IEnumerable<IInstanceClientFactory> instanceClientFactories = null,
        bool fatalIfCannotRegister = true
    ) {
        if (connector == null) {
            if (initialRegistryAddresses == null) {
                string registryAddresses = Environment.GetEnvironmentVariable(EnvironmentHelperConstants.EnvironmentVariables.InitialRegistryAddresses);
                initialRegistryAddresses = registryAddresses?.Split(';', StringSplitOptions.RemoveEmptyEntries);
            }

            if (instanceClientFactories == null) {
                instanceClientFactories = new[] {
                    XKit.Lib.Connector.Protocols.Http.HttpClientFactory.Factory
                };
            }
            connector = FabricConnectorFactory.Create(instanceClientFactories.ToList());

            if (dependencies != null) {
                Dependencies.AddRange(dependencies);
            }

            log.Value = LogSessionFactory.Factory.CreateLogSession();
            Log.Begin(LogContextTypeEnum.ClientAction, nameof(CreateInitConsumer));

            connector.Initialize();

            TaskUtil.RunAsyncAsSync(
                () => connector.Register(
                    Log,
                    initialRegistryAddresses,
                    new ConsumerXKitEnvironment(() => Dependencies, connector),
                    failIfUnableToRegister: fatalIfCannotRegister
                )
            );
        }
        return connector;
    }

    public void ResetDependencies(IEnumerable<IReadOnlyDescriptor> dependencies) {
        Dependencies.Clear();
        Dependencies.AddRange(dependencies);
    }

    public TServiceClientInterface CreateServiceClient<TServiceClientInterface>(
        IServiceClientFactory<TServiceClientInterface> factory,
        ILogSession log = null,
        ServiceCallTypeParameters defaultCallParameters = null
    ) {
        var useLog = log ?? Log;
        return factory.CreateServiceClient(
            useLog,
            Connector,
            defaultCallTypeParameters: defaultCallParameters ?? ServiceCallTypeParameters.SyncResult()
        );
    }

    public void Refresh(ILogSession log) 
        => TaskUtil.RunAsyncAsSync(() => connector.Refresh(log ?? Log));

    public void Unregister(ILogSession log) 
        => TaskUtil.RunAsyncAsSync(() => connector.Unregister(log ?? Log));

    public Task RefreshAsync(ILogSession log) 
        => connector.Refresh(log ?? Log);

    public async Task UnregisterAsync(ILogSession log) {
        await connector.Unregister(log ?? Log);
        Log.End(LogResultStatusEnum.Unknown);
    }
}

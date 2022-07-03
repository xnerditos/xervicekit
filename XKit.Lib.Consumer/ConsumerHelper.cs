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
using XKit.Lib.Common.Host;

namespace XKit.Lib.Consumer {

    public class ConsumerHelper {

        private readonly List<IReadOnlyDescriptor> Dependencies = new();
        
        private class SimpleFabricEnvironment : ILocalEnvironment {
            private readonly string fabricId = Identifiers.GenerateIdentifier();
            private readonly IDependencyConnector dependencyConnector; 

            private readonly Func<IEnumerable<IReadOnlyDescriptor>> getDependenciesSource;

            string ILocalEnvironment.FabricId => fabricId;

            ILogSessionFactory ILocalEnvironment.LogSessionFactory => LogSessionFactory.Factory;

            IDependencyConnector ILocalEnvironment.DependencyConnector => dependencyConnector;

            IHostEnvironment ILocalEnvironment.HostEnvironment => null;

            IEnumerable<IReadOnlyDescriptor> ILocalEnvironment.GetDependencies() 
                => getDependenciesSource?.Invoke()?.Select(d => d.Clone()).ToArray() ?? Array.Empty<Descriptor>();

            public SimpleFabricEnvironment(
                Func<IList<IReadOnlyDescriptor>> getDependenciesSource,
                IDependencyConnector dependencyConnector
            ) {
                this.getDependenciesSource = getDependenciesSource;
                this.dependencyConnector = dependencyConnector;
            }
        }

        private IFabricConnector fabricConnector;
        private readonly SetOnceOrThrow<ILogSession> log = new();
        
        public IDependencyConnector DependencyConnector => fabricConnector;
        public IFabricConnector FabricConnector => fabricConnector;
        public string FabricId => fabricConnector.FabricId;
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
            if (fabricConnector == null) {
                if (initialRegistryAddresses == null) {
                    string registryAddresses = Environment.GetEnvironmentVariable(EnvironmentHelperConstants.EnvironmentVariables.InitialRegistryAddresses);
                    initialRegistryAddresses = registryAddresses?.Split(';', StringSplitOptions.RemoveEmptyEntries);
                }

                if (instanceClientFactories == null) {
                    instanceClientFactories = new[] {
                        XKit.Lib.Connector.Protocols.Http.HttpClientFactory.Factory
                    };
                }
                fabricConnector = FabricConnectorFactory.Create(instanceClientFactories.ToList());

                if (dependencies != null) {
                    Dependencies.AddRange(dependencies);
                }

                log.Value = LogSessionFactory.Factory.CreateLogSession();
                Log.Begin(LogContextTypeEnum.ClientAction, nameof(CreateInitConsumer));

                fabricConnector.Initialize();

                TaskUtil.RunAsyncAsSync(
                    () => fabricConnector.Register(
                        Log,
                        initialRegistryAddresses,
                        new SimpleFabricEnvironment(() => Dependencies, fabricConnector),
                        failIfUnableToRegister: fatalIfCannotRegister
                    )
                );
            }
            return fabricConnector;
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
                DependencyConnector,
                defaultCallTypeParameters: defaultCallParameters ?? ServiceCallTypeParameters.SyncResult()
            );
        }

        public void Refresh(ILogSession log) 
            => TaskUtil.RunAsyncAsSync(() => fabricConnector.Refresh(log ?? Log));

        public void Unregister(ILogSession log) 
            => TaskUtil.RunAsyncAsSync(() => fabricConnector.Unregister(log ?? Log));

        public Task RefreshAsync(ILogSession log) 
            => fabricConnector.Refresh(log ?? Log);

        public async Task UnregisterAsync(ILogSession log) {
            await fabricConnector.Unregister(log ?? Log);
            Log.End(LogResultStatusEnum.Unknown);
        }
    }
}

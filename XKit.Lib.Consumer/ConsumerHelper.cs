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

    public static class ConsumerHelper {

        private static readonly List<IReadOnlyDescriptor> Dependencies = new();
        
        private class SimpleFabricEnvironment : ILocalEnvironment {
            private readonly string fabricId = Identifiers.GenerateIdentifier();
            private readonly IDependencyConnector dependencyConnector; 

            private readonly Func<IEnumerable<IReadOnlyDescriptor>> getDependenciesSource;

            string ILocalEnvironment.FabricId => fabricId;

            ILogSessionFactory ILocalEnvironment.LogSessionFactory => LogSessionFactory.Factory;

            IDependencyConnector ILocalEnvironment.DependencyConnector => dependencyConnector;

            IHostEnvironment ILocalEnvironment.HostEnvironment => null;

            IEnumerable<IReadOnlyDescriptor> ILocalEnvironment.GetDependencies() 
                => this.getDependenciesSource?.Invoke()?.Select(d => d.Clone()).ToArray() ?? Array.Empty<Descriptor>();

            public SimpleFabricEnvironment(
                Func<IList<IReadOnlyDescriptor>> getDependenciesSource,
                IDependencyConnector dependencyConnector
            ) {
                this.getDependenciesSource = getDependenciesSource;
                this.dependencyConnector = dependencyConnector;
            }
        }

        private static IFabricConnector fabricConnector;
        private static readonly SetOnceOrThrow<ILogSession> log = new();
        
        public static IDependencyConnector DependencyConnector => fabricConnector;
        public static IFabricConnector FabricConnector => fabricConnector;
        public static string FabricId => fabricConnector.FabricId;
        public static ILogSession Log => log.Value;

        public static IFabricConnector CreateInitConsumer(
            IEnumerable<IInstanceClientFactory> instanceClientFactories,
            IEnumerable<IServiceClientFactory> dependencyFactories,
            IEnumerable<string> initialRegistryAddresses = null,
            bool fatalIfCannotRegister = false
        ) {
            return CreateInitConsumer(
                instanceClientFactories, 
                dependencyFactories.Select(df => df.Descriptor),
                initialRegistryAddresses,
                fatalIfCannotRegister
            );
        }

        public static IFabricConnector CreateInitConsumer(
            IEnumerable<IInstanceClientFactory> instanceClientFactories = null,
            IEnumerable<IReadOnlyDescriptor> dependencies = null,
            IEnumerable<string> initialRegistryAddresses = null,
            bool fatalIfCannotRegister = true
        ) {
            if (ConsumerHelper.fabricConnector == null) {
                if (initialRegistryAddresses == null) {
                    string registryAddresses = Environment.GetEnvironmentVariable(EnvironmentHelperConstants.EnvironmentVariables.InitialRegistryAddresses);
                    initialRegistryAddresses = registryAddresses?.Split(';', StringSplitOptions.RemoveEmptyEntries);
                }

                if (instanceClientFactories == null) {
                    instanceClientFactories = new[] {
                        XKit.Lib.Connector.Protocols.Http.HttpClientFactory.Factory
                    };
                }
                ConsumerHelper.fabricConnector = FabricConnectorFactory.Create(instanceClientFactories.ToList());

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

        public static void ResetDependencies(IEnumerable<IReadOnlyDescriptor> dependencies) {
            Dependencies.Clear();
            Dependencies.AddRange(dependencies);
        }

        public static TServiceClientInterface CreateServiceClient<TServiceClientInterface>(
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

        public static void Refresh(ILogSession log) 
            => TaskUtil.RunAsyncAsSync(() => fabricConnector.Refresh(log ?? Log));

        public static void Unregister(ILogSession log) 
            => TaskUtil.RunAsyncAsSync(() => fabricConnector.Unregister(log ?? Log));

        public static Task RefreshAsync(ILogSession log) 
            => fabricConnector.Refresh(log ?? Log);

        public static async Task UnregisterAsync(ILogSession log) {
            await fabricConnector.Unregister(log ?? Log);
            Log.End(LogResultStatusEnum.Unknown);
        }
    }
}

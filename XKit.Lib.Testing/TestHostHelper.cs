using System.Collections.Generic;
using XKit.Lib.Host;
using XKit.Lib.Host.Config;
using XKit.Lib.Common.Log;
using System.Diagnostics;
using System;
using XKit.Lib.Common.Host;
using XKit.Lib.Connector.Protocols.Direct;
using XKit.Lib.Common.Registration;
using XKit.Lib.Testing.TestConfigSvc;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Threading;
using XKit.Lib.Testing.Mocking;
using Moq;
using XKit.Lib.Common.Utility.Threading;
using XKit.Lib.Testing.TestMessageBrokerSvc;
using XKit.Lib.Common.Utility.Extensions;
using System.IO;
using XKit.Lib.Common.Client;
using XKit.Lib.Connector.Protocols.Http;

namespace XKit.Lib.Testing {

    public static partial class TestHostHelper {

        public static readonly HostEnvironmentHelper HostEnvironmentHelper = new();
        private static readonly SemaphoreSlim synchronizer = new(1, 1);
        private static ILogSession log;
        private static bool daemonDebugMode = true;
        public static string LocalAddress { get; private set; }
        public static string LocalDataPath { get; private set; }
        public static IHostManager Host => HostEnvironmentHelper.Host;
        public static ILogSessionFactory LogSessionFactory => HostEnvironmentHelper.LogSessionFactory;
        public static IMessageBrokerSvcService TestMessageBrokerService { get; private set; }
        public static IHostEnvironment HostEnvironment => HostEnvironmentHelper.Host;
        public static ILocalEnvironment LocalEnvironment => HostEnvironmentHelper.Host;
        public static ILogSession Log {
            get {
                if (log == null) {
                    log = LogSessionFactory.CreateLogSession(
                        "test"
                    );
                    log.Begin(LogContextTypeEnum.DevelopmentTest);
                    log.Warning("Log session started on demand instead of in test runner");
                }
                return log;
            }
        }
        public static void Initialize(
            bool autoAddPlatformServices = true,
            bool loadMetaServices = false,
            bool useDaemonDebugMode = true
        ) {
            daemonDebugMode = useDaemonDebugMode;

            LocalAddress = "localhost";
            LocalDataPath = "./test-data-tmp/" + DateTime.Now.ToString("yyyy-MM-dd.HH.mm.ss.fff");

            HostEnvironmentHelper.CreateInitHost(
                instanceClientFactories: new IInstanceClientFactory[] { 
                    new DirectLocalClientFactory(),
                    new HttpClientFactory()
                },
                logSessionFactory: LogSessionFactory,
                localConfigSessionFactory: LocalConfigSessionFactory.Factory,
                hostAddress: LocalAddress,
                localDataStorageFolderPath: LocalDataPath,
                capabilitiesToRegister: loadMetaServices ? null : Array.Empty<string>() // empty array is "none"
            );

            if (autoAddPlatformServices) {
                AddService(
                    TestRegistrySvc.RegistrySvcServiceFactory.Create(LocalEnvironment)
                );
                
                AddService(
                    ConfigSvcServiceFactory.Create(LocalEnvironment)
                );

                TestMessageBrokerService = (IMessageBrokerSvcService) AddService(
                    MessageBrokerSvcServiceFactory.Create(LocalEnvironment)
                );
            }
        }

        /// <summary>
        /// Adds a mock service to the test environment
        /// </summary>
        /// <param name="descriptor">The Descriptor for the service being mocked</param>
        /// <param name="createMockApiOperation">a delegate that returns a mock api operation</param>
        /// <typeparam name="TApiInterface">The operation interface for the mock operation</typeparam>
        /// <returns></returns>
        public static IMockService<TApiInterface> AddMockService<TApiInterface>(
            IReadOnlyDescriptor descriptor, 
            Mock<TApiInterface> apiMock
        ) where TApiInterface : class, IServiceApi {
            var service = new MockService<TApiInterface>(
                descriptor,
                apiMock,
                LocalEnvironment
            );
            Host.AddManagedService(service);
            return service;
        }

        /// <summary>
        /// Adds a mock service to the test environment
        /// </summary>
        /// <param name="descriptor">The Descriptor for the service being mocked</param>
        /// <param name="createMockApiOperation">a delegate that returns a mock api operation</param>
        /// <typeparam name="TApiInterface">The operation interface for the mock operation</typeparam>
        /// <returns></returns>
        public static IMockService<TApiInterface> AddMockService<TApiInterface>(
            IReadOnlyDescriptor descriptor, 
            MockBehavior mockBehavior = MockBehavior.Loose
        ) where TApiInterface : class, IServiceCallable {
            var service = new MockService<TApiInterface>(
                descriptor,
                LocalEnvironment,
                mockBehavior
            );
            Host.AddManagedService(service);
            return service;
        }

        public static IManagedService AddService(
            IManagedService service
        ) {
            if (daemonDebugMode) {
                service.GetDaemons().ForEach(d => d.SetDebugMode(true));
            }
            Host.AddManagedService(
                service
            );
            return service;
        }

        public static IManagedService AddCreateService(
            IReadOnlyDescriptor descriptor,
            Type apiOperationInterfaceType
        ) {

            var service = Host.AddCreateManagedService(
                descriptor, 
                apiOperationInterfaceType
            );
            if (daemonDebugMode) {
                service.GetDaemons().ForEach(d => d.SetDebugMode(true));
            }
            return service;
        }

        public static void StartHost(
            Dictionary<string,object> hostStartupParams = null
        ) {

            Host.StartHost(
                initialRegistryHostAddresses: new[] { LocalAddress }, 
                startupParameters: hostStartupParams, 
                failIfCannotRegister: false
            );
        }

        public static void DestroyHost(bool cleanUpData = true) {
            HostEnvironmentHelper.StopAndDestroyHost();
            if (cleanUpData) {
                foreach(var f in Directory.EnumerateFiles(LocalDataPath, "*.*", new EnumerationOptions { RecurseSubdirectories = true })) {
                    try { File.Delete(f); } 
                    catch {}
                }
            }
        }

        public static void SetRuntimeConfiguration(
            HostConfigDocument hostConfig = null,
            IDictionary<IReadOnlyDescriptor, object> servicesConfig = null,
            bool clearAllExisting = true
        ) {
            if (clearAllExisting) { 
                ConfigSvcService.ClearAllExisting();
            }
            
            if (hostConfig != null) {
                ConfigSvcService.SetConfigForHost(hostConfig);
            }
            if (servicesConfig != null) {
                foreach(var kv in servicesConfig) {
                    ConfigSvcService.SetConfigForService(kv.Key, kv.Value);
                }
            }

            TaskUtil.RunAsyncAsSync(() => Host?.RefreshConfigurationFromSource());
        }

        public static async Task RunTestAsync(
            Func<Task> action,
            [CallerMemberName] string testName = null
        ) {
            await synchronizer.WaitAsync();
            try {
                string separator = new String('=', testName.Length + 22);                
                WriteLineConsole("");
                WriteLineConsole(separator);
                WriteLineConsole($"========== {testName} ==========");
                WriteLineConsole(separator);
                log = LogSessionFactory.CreateLogSession("TEST: " + testName);
                log.Begin(LogContextTypeEnum.DevelopmentTest);
                await action();
                //WriteLineConsole("---------------------------------------------");
                WriteLineConsole("");
            } catch(Exception ex) {
                WriteLineConsole("EXCEPTION!!! -->");
                WriteLineConsole(ex.Message);
                WriteLineConsole(ex.StackTrace);
                WriteLineConsole("<--");
                throw;
            } finally {
                log.End(LogResultStatusEnum.Unknown);
                log = null;
                synchronizer.Release();
            } 
        }

        public static void RunTest(
            Action action,
            [CallerMemberName] string testName = null
        ) {
            synchronizer.Wait();
            try {
                string separator = new string('=', testName.Length + 22);                
                WriteLineConsole("");
                WriteLineConsole(separator);
                WriteLineConsole($"========== {testName} ==========");
                WriteLineConsole(separator);
                log = LogSessionFactory.CreateLogSession("TEST: " + testName);
                log.Begin(LogContextTypeEnum.DevelopmentTest);
                action();
                //WriteLineConsole("---------------------------------------------");
                WriteLineConsole("");
            } catch(Exception ex) {
                WriteLineConsole("EXCEPTION!!! -->");
                WriteLineConsole(ex.Message);
                WriteLineConsole(ex.StackTrace);
                WriteLineConsole("<--");
                throw;
            } finally {
                log.End(LogResultStatusEnum.Unknown);
                log = null;
                synchronizer.Release();
            } 
        }

        public static void WriteLineConsole(string message) {
            Console.WriteLine(message);
            Debug.WriteLine(message);
        }
    }
}

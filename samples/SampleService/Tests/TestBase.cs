using System.Collections.Generic;
using XKit.Lib.Common.Fabric;
using Samples.SampleService.V1.Client;
using Samples.SampleService.V1;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;
using XKit.Lib.Testing;
using XKit.Lib.Testing.TestMessageBrokerSvc;

namespace Tests.Services.SampleService {

    public class TestBase {

        private static bool isInited = false;
        protected static readonly TestHostHelper TestHelper = new();
        protected static IFabricConnector Connector => TestHelper.HostEnvironment.Connector;
        protected static IXKitHostEnvironment HostEnvironment => TestHelper.HostEnvironment;
        protected string FabricId => TestHelper.Host.FabricId;
        protected static IMessageBrokerSvcService MessageBroker => TestHelper.TestMessageBrokerService;
        
        protected static void ClassInit() {
            
            if (isInited) { return; }
            isInited = true;
            TestHelper.InitializeLocalTestHost();

            TestHelper.AddService(
                new SampleServiceService(HostEnvironment)
            );

            TestHelper.StartHost();
        }

        protected static void ClassTeardown() {            
            if (!isInited) { return; }
            isInited = false;
            TestHelper.DestroyHost();
        }

        protected static void SetRuntimeConfiguration(
            HostConfigDocument hostConfig = null,
            IDictionary<IReadOnlyDescriptor, object> servicesConfig = null
        ) => TestHelper.SetRuntimeConfiguration(hostConfig, servicesConfig);
        
        protected ISampleServiceApi CreateClient() {
            return new SampleServiceClient(TestHelper.Log, Connector, ServiceCallTypeParameters.SyncResult());
        }
    }
}

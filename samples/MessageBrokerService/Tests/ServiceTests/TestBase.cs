using System.Collections.Generic;
using XKit.Lib.Common.Fabric;
using Samples.MessageBroker;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;
using XKit.Lib.Testing;
using Samples.MessageBroker.Tests.TestServices;
using Samples.MessageBroker.Engine;
using Samples.MessageBroker.Daemons;
using Constants = Samples.MessageBroker.Constants;
using XKit.Lib.Common.Log;
using System.Diagnostics;
using XKit.Lib.Common.Services.MessageBroker;
using Samples.MessageBroker.Client;

namespace Samples.MessageBroker.Tests;

public class TestBase {

    private static volatile bool isInited = false;

    private static readonly TestHostHelper TestHelper = new();
    protected static IXKitHostEnvironment HostEnvironment => TestHelper.HostEnvironment;
    protected IFabricConnector Connector => HostEnvironment.Connector;
    protected ILogSession Log => TestHelper.Log;
    protected static IMessageEngine Engine { get; private set; }
    protected static Svc1Service Svc1 { get; private set; }
    protected static Svc2Service Svc2 { get; private set; }
    protected static Svc3Service Svc3 { get; private set; }
    protected static Svc4Service Svc4 { get; private set; }
    protected static IXKitHost Host => TestHelper.Host;
    protected static IDeliveryDaemon Daemon { get; private set; }
    protected static IMessageBrokerService MessageBrokerService { get; private set; }

    protected static void InitTests() {
        
        if (isInited) { return; }
        isInited = true;
        TestHelper.InitializeLocalTestHost(autoAddPlatformServices: false);
        
        TestHelper.AddService(
            XKit.Lib.Testing.TestRegistrySvc.RegistrySvcServiceFactory.Create(HostEnvironment)
        );
            
        TestHelper.AddService(
            XKit.Lib.Testing.TestConfigSvc.ConfigSvcServiceFactory.Create(HostEnvironment)
        );

        MessageBrokerService = new MessageBrokerService(HostEnvironment);
        TestHelper.AddService(MessageBrokerService);

        Svc1 = new Svc1Service(HostEnvironment);
        Svc2 = new Svc2Service(HostEnvironment);
        Svc3 = new Svc3Service(HostEnvironment);
        Svc4 = new Svc4Service(HostEnvironment);

        TestHelper.AddService(Svc1);
        TestHelper.AddService(Svc2);
        TestHelper.AddService(Svc3);
        TestHelper.AddService(Svc4);
        
        TestHelper.StartHost();
        
        Daemon = (IDeliveryDaemon)Host.GetManagedService(Constants.ServiceDescriptor).GetDaemon(nameof(DeliveryDaemon));
        Engine = MessageBrokerService.GetMessageEngine();
    }

    protected static void TeardownTests() {            
        if (!isInited) { return; }
        isInited = false;
        TestHelper.DestroyHost();
    }

    protected static void SetRuntimeConfiguration(
        HostConfigDocument hostConfig = null,
        IDictionary<IReadOnlyDescriptor, object> servicesConfig = null
    ) => TestHelper.SetRuntimeConfiguration(hostConfig, servicesConfig);
    
    protected IMessageBrokerApi CreateClient() {
        return new MessageBrokerClient(Log, Connector);
    }

    protected void DispatchDeliveryDaemonMessages(int? count = null) {

        count ??= 10;
        int i = 0;
        for(; i < count && Daemon.DebugProcessOneMessage(); i++) { }
        if (i > 10) {
            Debug.Fail("Message processing seems to be on an infinite loop");
        }
    } 

    protected void DispatchDeliveryDaemonMessagesAndTimerEvents(int cycleCount = 3) {
        for(int i = 0; i < cycleCount; i++) {
            DispatchDeliveryDaemonMessages();
            Daemon.DebugFireTimerEvent();
        }
        DispatchDeliveryDaemonMessages();
    }
}

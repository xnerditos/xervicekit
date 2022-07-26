using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XKit.Lib.Common.Config;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Services.MessageBroker;
using XKit.Lib.Common.Utility;
using XKit.Lib.Host.DefaultBaseClasses;
using Samples.MessageBroker.Daemons;
using Samples.MessageBroker.Engine;
using Mapster;
using XKit.Lib.Host.Services;
using StandardServices = XKit.Lib.Common.Services.StandardConstants.Managed.StandardServices;

namespace Samples.MessageBroker;
public class MessageBrokerService : ManagedService<MessageBrokerOperation>, IMessageBrokerService {

    private readonly MessageBrokerConfig DefaultConfig = new();
    private readonly Engine.MessageEngine engine = new();

    // NOTE:  The Message Broker service, being one of a handful of "Platform" services, 
    //        uses the standard descriptor that the framework knows about.
    private static readonly IReadOnlyDescriptor descriptor = StandardServices.MessageBroker.Descriptor;

    private readonly SetOnceOrThrow<IConfigReader<MessageBrokerConfig>> configReader = new();
    private readonly SetOnceOrThrow<IDeliveryDaemon> deliveryDaemon = new();
    private IDeliveryDaemon DeliveryDaemon => deliveryDaemon.Value;
    private readonly SetOnceOrThrow<IGenericTimerDaemon> housekeepingDaemon = new();
    private IGenericTimerDaemon HousekeepingDaemon => housekeepingDaemon.Value;
    private IConfigReader<MessageBrokerConfig> ConfigReader {
        get => configReader.Value;
        set => configReader.Value = value;
    }

    // =====================================================================
    // overrides
    // =====================================================================

    protected override IReadOnlyDescriptor Descriptor => descriptor;

    // =====================================================================
    // construction
    // =====================================================================

    public MessageBrokerService(
        IXKitHostEnvironment hostEnvironment
    ) : base(hostEnvironment) {

        this.OnServiceStartingEvent += this.OnServiceStart;
        this.OnServiceStoppingEvent += this.OnServiceStop;
        this.deliveryDaemon.Value = new DeliveryDaemon(HostEnvironment.LogSessionFactory);
        this.AddDaemon(this.DeliveryDaemon);
        this.housekeepingDaemon.Value = 
            new GenericTimerDaemon<HousekeepingDaemonOperation>(
                logSessionFactory: HostEnvironment.LogSessionFactory,
                timerDelayMilliseconds: 60 * 1000,
                name: "HousekeepingDaemon",
                onEnvironmentChangeHandler: (daemon) => {
                    var cfg = ConfigReader?.GetConfig(DefaultConfig);
                    daemon.SetTimerDelay((cfg?.HousekeepingDaemon?.CheckEveryXSeconds ?? 60) * 1000);
                    daemon.SetTimerEnabled(cfg?.HousekeepingDaemon.Enable ?? true);
                }
            );

        this.AddDaemon(this.HousekeepingDaemon);
    }

    // =====================================================================
    // IMessageBrokerService
    // =====================================================================

    IMessageEngine IMessageBrokerService.GetMessageEngine() => engine;
    
    Task<MessageBrokerConfig> IMessageBrokerService.GetConfig() {
        return Task.FromResult(ConfigReader.GetConfig(DefaultConfig));
    }

    void IMessageBrokerService.SignalMessageAdded(IMessageRecord record, IReadOnlyList<Guid> queueItemIdsAdded) {
        foreach(var id in queueItemIdsAdded) {
            DeliveryDaemon.PostMessage(new() { QueueItemId = id }, triggerProcessing: false);
        }
        DeliveryDaemon.ProcessMessages(background: true);
    }

    // =====================================================================
    // Events
    // =====================================================================

    private void OnServiceStart(ILogSession log) {
		ConfigReader = AddFeatureConfigurable<MessageBrokerConfig>();
        SetupMappings();   
        var cfg = ConfigReader?.GetConfig(DefaultConfig);
        HousekeepingDaemon.SetTimerDelay((cfg?.HousekeepingDaemon?.CheckEveryXSeconds ?? 60) * 1000);
        HousekeepingDaemon.SetTimerEnabled(cfg?.HousekeepingDaemon.Enable ?? true);
    }

    private void OnServiceStop(ILogSession _) { }

    // =====================================================================
    // private 
    // =====================================================================

    private static void SetupMappings() {
			TypeAdapterConfig<IReadOnlyFabricMessage, FabricMessage>
				.ForType()
				.Compile();
			TypeAdapterConfig<IReadOnlySubscription, Subscription>
				.ForType()
				.Compile();
			TypeAdapterConfig<IMessageRecord, MessageRecord>
				.ForType()
				.Compile();
			TypeAdapterConfig<IQueueRecord, QueueRecord>
				.ForType()
				.Compile();
			TypeAdapterConfig<IQueueItemRecord, QueueItemRecord>
				.ForType()
				.Compile();
    }
}

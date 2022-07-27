using System.Threading.Tasks;
using XKit.Lib.Host.DefaultBaseClasses;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Client;
using XKit.Lib.Connector.Service;
using XKit.Lib.Common.Utility;
using XKit.Lib.Common.Fabric;
using System;
// TODO:  Uncomment if database access is needed via the repository
//using XKit.Lib.Data.DocumentRepository;

namespace Samples.SampleService.V1; 

public partial class SampleServiceOperation : ServiceOperation<ISampleServiceService>, ISampleServiceApi, IServiceOperation {

    // TODO:  Add service dependencies class level fields and init according to the demo pattern 
    //private readonly SetOnceOrThrow<IUserClient> userService = new();
    //private IUserClient UserService => userService.Value;

    private SampleServiceConfig config;

    // TODO:  Remove if not using events
    private readonly SetOnceOrThrow<IEventMessenger<SampleServiceEvents>> eventMessenger = new();
    private IEventMessenger<SampleServiceEvents> EventMessenger => eventMessenger.Value;

    public SampleServiceOperation(
        ServiceOperationContext context
    ) : base(
        context
    ) { 
    }

    // =====================================================================
    // ISampleServiceOperation
    // =====================================================================

    // NOTE:  Implementations for methods in ISampleServiceOperation will be in 
    //        separate 'partial class' files named by the pattern: 
    //		  Operation.MethodName.cs  Keep related worker methods, 
    //		  etc together in the same file. 

    // =====================================================================
    // general workers
    // =====================================================================

    protected override async Task<bool> InitServiceOperation() {
        try {
            config = await Service.GetConfig();

            // TODO:  Init service dependencies according to the demo pattern 
            //userService.Value = new UserClient(
            //    Log,
            //    this.HostEnvironment.Connector
            //    ServiceCallTypeParameters p = ServiceCallTypeParameters.SyncResult
            //);

            // TODO:  Remove if not using events
            eventMessenger.Value = ClientFactory.Factory.CreateEventMessengerClient<SampleServiceEvents>(
                Log,
                HostEnvironment.Connector
            );
            return true;
        } catch (Exception ex) {
            Log.Fatality(ex.Message);
            return false;
        }
    }
}

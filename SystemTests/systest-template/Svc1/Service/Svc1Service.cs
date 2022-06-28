using XKit.Lib.Common.Registration;
using XKit.Lib.Host.DefaultBaseClasses;
using XKit.Lib.Common.Config;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Utility;
using System.Threading.Tasks;
using SystemTests._NAMESPACE.Svc1.Entities;
using System;

namespace SystemTests._NAMESPACE.Svc1.Service {

    public interface ISvc1Service : IManagedService, IServiceBase {
        // TODO:
        //Task<Svc1Config> GetConfig(Svc1Config defaultValue = default(Svc1Config));
    }

    public class Svc1Config {

    }

	public class Svc1Service 
        : ManagedService<Svc1Operation>, ISvc1Service {

		private static readonly IReadOnlyDescriptor descriptor = Constants.ServiceDescriptor;
		
		private readonly SetOnceOrThrow<IConfigReader<Svc1Config>> configReader = new();

		private IConfigReader<Svc1Config> ConfigReader {
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

		public Svc1Service(
            ILocalEnvironment localEnvironment
		) : base(localEnvironment) { }
		// TODO:
        //     this.OnServiceStartEvent += this.OnServiceStart;
        //     this.OnServiceStopEvent += this.OnServiceStop;
		// }

		// // =====================================================================
		// // ISvc1Service
		// // =====================================================================

        // async Task<Svc1Config> ISvc1Service.GetConfig(Svc1Config defaultValue) {
        //     return await ConfigReader.GetConfig(defaultValue);
        // }
		
        // // =====================================================================
        // // Events
        // // =====================================================================
        
        // private void OnServiceStart() {

		// 	ConfigReader = AddFeatureConfigurable<Svc1Config>();	
        // }

        // private void OnServiceStop() { }
	}

    public partial class Svc1Operation : ServiceOperation<ISvc1Service>, ISvc1Api {

        public Svc1Operation(
            ServiceOperationContext context
        ) : base(context) { }


        // ---------------------------------------------------------------------
        // ISvc1.GetTestValue
        // ---------------------------------------------------------------------
        async Task<ServiceCallResult<TestValueResponse>> ISvc1Api.GetTestValue(TestValueRequest request) {
            return await RunServiceCall(
                request,
                operationAction: (r) => Task.FromResult(new TestValueResponse {
                    TheIncomingValue = r.TheValue,
                    RandomValue = Guid.NewGuid().ToString()
                })
            );
        }
    }
}

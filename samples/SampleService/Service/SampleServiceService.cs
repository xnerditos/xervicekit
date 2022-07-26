using System.Collections.Generic;
using System.Threading.Tasks;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Config;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Utility;
using XKit.Lib.Host.DefaultBaseClasses;

namespace Samples.SampleService.V1 {

	public class SampleServiceService : ManagedService<SampleServiceOperation>, ISampleServiceService {

		private static readonly IReadOnlyDescriptor descriptor = Constants.ServiceDescriptor;
		
		private SetOnceOrThrow<IConfigReader<SampleServiceConfig>> configReader = new SetOnceOrThrow<IConfigReader<SampleServiceConfig>>();
		private IConfigReader<SampleServiceConfig> ConfigReader {
            get => configReader.Value;
            set => configReader.Value = value;
        }

		// =====================================================================
		// overrides
		// =====================================================================

		protected override IReadOnlyDescriptor Descriptor => descriptor;

        protected override IEnumerable<IReadOnlyDescriptor> Dependencies => new IReadOnlyDescriptor[] {
			// TODO:  Add dependencies or delete this
		};

		// =====================================================================
		// construction
		// =====================================================================

		public SampleServiceService(
            IXKitHostEnvironment hostEnvironment
		) : base(hostEnvironment) {
		
            // TODO:  Delete these if not necessary
            this.OnServiceStartingEvent += OnServiceStart;
            this.OnServiceStoppingEvent += OnServiceStop;
            AddDaemon(new SampleDaemon());
            
            // Note:  Only simple construction should go here.  Significant init should go
            //        in OnServiceStart
		}

		// =====================================================================
		// ISampleServiceService
		// =====================================================================

        Task<SampleServiceConfig> ISampleServiceService.GetConfig(SampleServiceConfig defaultValue) {
            return Task.FromResult(ConfigReader.GetConfig(defaultValue));
        }

        // =====================================================================
        // Events
        // =====================================================================
        
        private void OnServiceStart(ILogSession log) {
			ConfigReader = AddFeatureConfigurable<SampleServiceConfig>();
        }

        private void OnServiceStop(ILogSession log) {
            // TODO:  Put any service level cleanup here
		}
	}
}

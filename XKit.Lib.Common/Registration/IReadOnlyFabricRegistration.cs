using System.Collections.Generic;

namespace XKit.Lib.Common.Registration {

	public interface IReadOnlyFabricRegistration {
		/// <summary>
		/// An globally unique id for this process.  ID's should not be recycled 
		/// if the process dies.
		/// </summary>
		string FabricId { get; }
        IReadOnlyFabricStatus Status { get; }
		IEnumerable<IReadOnlyServiceRegistration> HostedServices { get; }
		IEnumerable<IReadOnlyDescriptor> Dependencies { get; }
		string Address { get; set; }
		IEnumerable<string> Capabilities { get; }
        FabricRegistration Clone();
	}
}
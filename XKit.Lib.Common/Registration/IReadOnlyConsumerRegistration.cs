using System.Collections.Generic;

namespace XKit.Lib.Common.Registration {

	public interface IReadOnlyConsumerRegistration {
		/// <summary>
		/// An globally unique id for this process.  ID's should not be recycled 
		/// if the process dies.
		/// </summary>
		string FabricId { get; }
		IEnumerable<IReadOnlyDescriptor> Dependencies { get; }
        ConsumerRegistration Clone();
	}
}
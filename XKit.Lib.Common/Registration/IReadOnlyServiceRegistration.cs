using System.Collections.Generic;

namespace XKit.Lib.Common.Registration {
	public interface IReadOnlyServiceRegistration {
		string RegistrationKey { get; }
		IReadOnlyDescriptor Descriptor { get; }
		IReadOnlyServiceCallPolicy CallPolicy { get; }
        IEnumerable<IReadOnlyServiceInstance> Instances { get; }
        ServiceRegistration Clone();
	}
}
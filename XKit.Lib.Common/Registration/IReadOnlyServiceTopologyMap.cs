using System;
using System.Collections.Generic;

namespace XKit.Lib.Common.Registration
{
	/// <summary>
	/// This class conveys the information necessary for a consumer to
	/// consume system services
	/// </summary>
	public interface IReadOnlyServiceTopologyMap {
		IEnumerable<IReadOnlyServiceRegistration> Services { get; }
		DateTime? CacheExpiration { get; }
        ServiceTopologyMap Clone();
	}
}
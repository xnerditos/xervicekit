using System.Collections.Generic;
using System.Threading.Tasks;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Registration;

namespace XKit.Lib.Common.Fabric {

    /// <summary>
    /// Implementers will contain the logic for routing a service call to the 
    /// appropriate instance
    /// </summary>
    public interface IServiceCallRouter {

        Task<IReadOnlyList<ServiceCallResult>> ExecuteCall(
            ServiceCallRequest request,
            ILogSession log,
            ServiceCallPolicy policy = null,
            string targetHostId = null
        );

        /// <summary>
        /// Causes this router to become invalid
        /// </summary>
        void Invalidate();

        bool IsValid { get; }

        string ServiceRegistrationKey { get; }

        IReadOnlyDescriptor ServiceDescriptor { get; }
    }
}

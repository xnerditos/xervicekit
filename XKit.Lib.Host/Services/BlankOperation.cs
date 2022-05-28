using XKit.Lib.Common.Host;
using XKit.Lib.Host.DefaultBaseClasses;

namespace XKit.Lib.Host.Services {

    internal interface IBlankApi : IServiceOperation, IServiceApi { }

    internal partial class BlankServiceOperation : ServiceOperation, IBlankApi {

        public BlankServiceOperation(
            ServiceOperationContext context
        ) : base(
            context
        ) { }
    }
}
using System.Collections.Generic;
using XKit.Lib.Common.Utility;

namespace XKit.Lib.Common.Registration {

    public class DescriptorEqualityComparer : IEqualityComparer<Descriptor> {
        bool IEqualityComparer<Descriptor>.Equals(Descriptor x, Descriptor y) 
            => x.IsMetaService == y.IsMetaService &&
               x.Collection == y.Collection &&
               x.Name == y.Name &&
               x.Version == y.Version &&
               x.UpdateLevel == y.UpdateLevel &&
               x.PatchLevel == y.PatchLevel;

        int IEqualityComparer<Descriptor>.GetHashCode(Descriptor obj) 
            => Identifiers.GetServiceFullRegistrationKey(obj).GetHashCode();
    }

    public class ReadOnlyDescriptorEquialityComparer : IEqualityComparer<IReadOnlyDescriptor> {
        bool IEqualityComparer<IReadOnlyDescriptor>.Equals(IReadOnlyDescriptor x, IReadOnlyDescriptor y) 
            => x.IsMetaService == y.IsMetaService &&
               x.Collection == y.Collection &&
               x.Name == y.Name &&
               x.Version == y.Version &&
               x.UpdateLevel == y.UpdateLevel &&
               x.PatchLevel == y.PatchLevel;

        int IEqualityComparer<IReadOnlyDescriptor>.GetHashCode(IReadOnlyDescriptor obj) 
            => Identifiers.GetServiceFullRegistrationKey(obj).GetHashCode();
    }
}
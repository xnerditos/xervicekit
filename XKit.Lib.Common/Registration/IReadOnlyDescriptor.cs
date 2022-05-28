
namespace XKit.Lib.Common.Registration {

	public interface IReadOnlyDescriptor {
        bool IsMetaService { get; }
		string Collection { get; }
		string Name { get; }
		int Version { get; }
		int UpdateLevel { get; }
		int PatchLevel { get; }
        Descriptor Clone();
		bool IsSameService(IReadOnlyDescriptor comparison);
		bool MeetsRequirement(IReadOnlyDescriptor requirement);
	}
}
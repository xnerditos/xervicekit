
namespace XKit.Lib.Common.Registration {

	public partial class Descriptor : IReadOnlyDescriptor {

        public bool IsMetaService { get; set; }

		public string Collection { get; set; }

		public string Name { get; set; }

		public int Version { get; set; }

		public int UpdateLevel { get; set; }

		public int PatchLevel { get; set; }

        public Descriptor Clone() 
            => new() {
                Collection = Collection,
                Name = Name,
                Version = Version,
                UpdateLevel = UpdateLevel,
                PatchLevel = PatchLevel
            };

		public bool IsSameService(IReadOnlyDescriptor comparison)
            =>  IsMetaService == comparison.IsMetaService &&
                Collection == comparison.Collection &&
                Name == comparison.Name &&
                Version == comparison.Version;

		public bool MeetsRequirement(IReadOnlyDescriptor requirement) 
            =>  IsSameService(requirement) &&
				(UpdateLevel > requirement.UpdateLevel ||
				    (UpdateLevel == requirement.UpdateLevel && PatchLevel >= requirement.PatchLevel));
	}
}

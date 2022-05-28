using System;
using XKit.Lib.Common.Registration;

namespace XKit.Lib.Common.Utility {
    
    public static class Identifiers {
        public const char KeyPartSeparationCharacter = '.';
        public const string NameOriginatorAsHost = "host";
        public const string NameOriginatorAsTestHost = "test-host";
        public static string GenerateIdentifier() 
            => Guid.NewGuid().ToString("N");

        /// <summary>
        /// Creates a unique key for the service using all registration information including patch level
        /// </summary>
        public static string GetServiceFullRegistrationKey(IReadOnlyDescriptor descriptor) 
            => $"{GetServiceVersionLevelKey(descriptor)}{KeyPartSeparationCharacter}{descriptor.UpdateLevel}{KeyPartSeparationCharacter}{descriptor.PatchLevel}";

        /// <summary>
        /// Creates a unique key for the service using all registration information including patch level
        /// </summary>
        public static string GetServiceRegistrationKey(IReadOnlyServiceRegistration registration) 
            => GetServiceFullRegistrationKey(registration.Descriptor);

        /// <summary>
        /// Creates a unique key for the service / version combination
        /// </summary>
        public static string GetServiceVersionLevelKey(IReadOnlyDescriptor descriptor) 
            => $"{descriptor.Collection?.ToLower() ?? XKit.Lib.Common.Services.StandardConstants.Managed.Collections.Blank}{KeyPartSeparationCharacter}{descriptor.Name.ToLower()}{KeyPartSeparationCharacter}{descriptor.Version}";

        /// <summary>
        /// Creates a unique key for the host / library version combination
        /// </summary>
        public static string GetHostVersionLevelKey(int versionLevel) 
            => $"{NameOriginatorAsHost}v{versionLevel}";
    }
}

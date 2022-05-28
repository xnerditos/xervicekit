using System;
using XKit.Lib.Common.Config;
using System.Text;
using System.Collections.Generic;
using System.IO;

namespace XKit.Lib.Host.Config {

	public class LocalConfigSessionFactory : ILocalConfigSessionFactory {

        private static readonly HashSet<char> disallowedFilenameCharacters = new HashSet<char>(
            Path.GetInvalidFileNameChars()
        );
        private const char replacementFilenameChar = '_';

		private static ILocalConfigSessionFactory factory = new LocalConfigSessionFactory();
		public static ILocalConfigSessionFactory Factory => factory;

        private string localConfigFolderPath;

        public LocalConfigSessionFactory() {
            string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            localConfigFolderPath = userFolder;
        }

		// =====================================================================
		// ILocalConfigSessionFactory
		// =====================================================================

        void ILocalConfigSessionFactory.SetPath(
            string localConfigFolderPath
        ) {
            this.localConfigFolderPath = localConfigFolderPath;
        }

		ILocalConfigSession ILocalConfigSessionFactory.Create(
            string configDocumentIdentifier,
            string extendedName
        ) => CreateConcreteClass(configDocumentIdentifier, extendedName);

		// =====================================================================
		// workers
		// =====================================================================

		private ILocalConfigSession CreateConcreteClass(
            string configDocumentIdentifier,
            string extendedName
        ) {
            configDocumentIdentifier = configDocumentIdentifier ?? throw new ArgumentNullException(nameof(configDocumentIdentifier));
            var filename = SanitizeFilename(configDocumentIdentifier);
            var extendedNameForFile = "";
            if (!string.IsNullOrEmpty(extendedName)) {
                extendedNameForFile = $".{SanitizeFilename(extendedName)}";
            }
			if (string.IsNullOrEmpty(localConfigFolderPath)) {
				throw new ArgumentException($"{nameof(localConfigFolderPath)} must be set before calling Create");
			}
			return new LocalConfigSession(
				$"{localConfigFolderPath}/{filename}{extendedNameForFile}.json"
            );
		}

        private string SanitizeFilename(string f) {
            var newFilename = new StringBuilder(f.Length);
            foreach(var c in f) {
                newFilename.Append(
                    disallowedFilenameCharacters.Contains(c) ? replacementFilenameChar : c
                );
            }
            return newFilename.ToString();
        }

		// =====================================================================
		// Static
		// =====================================================================

		public static ILocalConfigSession Create(
            string configDocumentIdentifier,
            string extendedName = null
        ) => Factory.Create(configDocumentIdentifier, extendedName);

        public static void SetPath(
            string localConfigFolderPath
        ) => Factory.SetPath(localConfigFolderPath);
            
		public static void InjectCustomFactory(ILocalConfigSessionFactory factory) 
			=> LocalConfigSessionFactory.factory = factory;
    }
}
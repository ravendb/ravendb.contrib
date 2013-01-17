using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Win32;
using Raven.Abstractions.Data;
using Raven.Database;

namespace Raven.Bundles.IndexedAttachments
{
    internal static class Utils
    {
        public static Version RavenVersion
        {
            get
            {
                var assembly = typeof(DocumentDatabase).Assembly;
                var attribute = assembly.GetCustomAttributes(false).OfType<AssemblyFileVersionAttribute>().First();
                return new Version(attribute.Version);
            }
        }

        public static string GetMimeType(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            if (extension == null) return null;

            using (var regKey = Registry.ClassesRoot.OpenSubKey(extension.ToLower(), false))
            {
                if (regKey == null)
                    return null;

                return regKey.GetValue("Content Type") as string;
            }
        }

        /// <summary>
        /// Attempts to determine the filename by convention.
        /// Assumes the last part of the key to be a filename if it ends with a 3 or 4 digit extension.
        /// </summary>
        /// <param name="key"></param>
        public static string GetFilename(string key)
        {
            var i = key.LastIndexOf('.');
            return i != -1 && (i == key.Length - 4 || i == key.Length - 5)
                       ? key.Split('/').Last()
                       : null;
        }

        public static bool IsBundleActive(this DocumentDatabase database, string bundleName)
        {
            var assembliesLoaded = AppDomain.CurrentDomain.GetAssemblies();
            var embeddedMode = assembliesLoaded.Any(x => x.GetName().Name.Contains("Raven.Client.Embedded"));
            if (embeddedMode)
                return true;

            var activeBundles = database.Configuration.Settings[Constants.ActiveBundles];
            return activeBundles != null && activeBundles.Split(';').Contains(bundleName, StringComparer.OrdinalIgnoreCase);
        }
    }
}

using System;
using System.Linq;
using Microsoft.Win32;

namespace SaneRegistry
{
    /// <summary>
    /// Exposes the validation methods used on registry paths and names.
    /// </summary>
    public static class RegistryPathValidator
    {
        // This type is loosely based on the specs provided at 
        // https://msdn.microsoft.com/en-us/library/windows/desktop/ms724872(v=vs.85).aspx. These
        // specifications provided the types of limitations to expect, but testing revealed that
        // the values specified were in fact incorrect.
        
        private static readonly int _maxTreeDepth = 510;
        private static readonly int _maxPathLength = 32724;
        private static readonly int _maxNameLength = 255;
        private static readonly string[] _baseNames;

        static RegistryPathValidator()
        {
            _baseNames =
                Enum.GetValues(typeof(RegistryHive))
                .Cast<RegistryHive>()
                .Select(hKey => RegistryKey.OpenBaseKey(hKey, RegistryView.Default))
                .Select(baseKey => baseKey.Name)
                .ToArray();
        }

        /// <summary>
        /// Validates absolute registry key paths.
        /// </summary>
        /// <param name="path">The path to validate.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="path"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="path"/> begins with an invalid base key name
        /// <para/>- or -<para/>
        /// <paramref name="path"/> contains more than 510 names
        /// <para/>- or -<para/>
        /// <paramref name="path"/> contains names that exceed 255 characters
        /// <para/>- or -<para/>
        /// <paramref name="path"/> is longer than 32724 characters
        /// </exception>
        public static void ValidateKeyPath(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            var names = path.Split('\\');
            if (!_baseNames.Contains(names[0].ToUpper()))
                throw new ArgumentException($"Invalid base key name '{names[0]}'.", nameof(path));

            if (names.Count() > _maxTreeDepth)
                throw new ArgumentException(
                    $"Path exceeds maximum tree depth ({_maxTreeDepth}).", nameof(path));

            var overLengthNames = names.Where(n => n.Length > _maxNameLength);
            if (overLengthNames.Count() > 0)
                throw new ArgumentException(
                    $"Path contains a name that exceeds the {_maxNameLength} limit: " +
                    $"'{overLengthNames.First()}'.",
                    nameof(path));

            if (path.Length > _maxPathLength)
                throw new ArgumentException(
                    $"Path exceeds maximum path length ({_maxPathLength}).", nameof(path));
        }
    }
}

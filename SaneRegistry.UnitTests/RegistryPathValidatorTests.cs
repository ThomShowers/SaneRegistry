using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SaneRegistry.UnitTests
{
    [TestClass]
    public class RegistryPathValidatorTests
    {
        private const string _validBaseKeyName = "HKEY_CURRENT_USER";
        private const string _invalidBaseKeyName = "HKEY_I_HOPE_THIS_IS_NEVER_A_REGISTRY_KEY_NAME";

        [TestMethod]
        public void ValidateRegistryKeyPath_NullPath_Throws()
        {
            var ex = 
                Assert.ThrowsException<ArgumentNullException>(
                    () => RegistryPathValidator.ValidateKeyPath(null));
            Assert.AreEqual("path", ex.ParamName);
        }

        [TestMethod]
        public void ValidateRegistryKeyPath_InvalidBaseKeyName_Throws()
        {
            var ex =
                Assert.ThrowsException<ArgumentException>(
                    () => RegistryPathValidator.ValidateKeyPath(_invalidBaseKeyName));
            Assert.AreEqual("path", ex.ParamName);
        }

        [DataTestMethod]
        [DataRow("HKEY_CLASSES_ROOT")]
        [DataRow("HKEY_CURRENT_CONFIG")]
        [DataRow("HKEY_CURRENT_USER")]
        [DataRow("HKEY_DYN_DATA")]
        [DataRow("HKEY_LOCAL_MACHINE")]
        [DataRow("HKEY_PERFORMANCE_DATA")]
        [DataRow("HKEY_USERS")]
        public void ValidateRegistryKeyPath_ValidBaseKeyName_DoesNotThrow(string path)
        {
            RegistryPathValidator.ValidateKeyPath(path);
        }

        [TestMethod]
        public void ValidateRegistryKeyPath_LowercaseValidBaseKeyName_DoesNotThrow()
        {
            RegistryPathValidator.ValidateKeyPath(_validBaseKeyName.ToLower());
        }

        [TestMethod]
        public void ValidateRegistryKeyPath_MoreThan510Levels_Throws()
        {
            var levels = 511;
            var keyPath =
                $@"{_validBaseKeyName}\{string.Join(@"\", Enumerable.Repeat('a', levels - 1))}";
            var ex =
                Assert.ThrowsException<ArgumentException>(
                    () => RegistryPathValidator.ValidateKeyPath(keyPath));
            Assert.AreEqual("path", ex.ParamName);
        }

        [TestMethod]
        public void ValidateRegistryKeyPath_510Levels_DoesNotThrow()
        {
            var levels = 510;
            var keyPath =
                $@"{_validBaseKeyName}\{string.Join(@"\", Enumerable.Repeat('a', levels - 1))}";
            RegistryPathValidator.ValidateKeyPath(keyPath);
        }

        [TestMethod]
        public void ValidateRegistryKeyPath_PathWithNameLongerThan255_Throws()
        {
            var path = $@"{_validBaseKeyName}\{new string(Enumerable.Repeat('a', 256).ToArray())}";
            var ex =
                Assert.ThrowsException<ArgumentException>(
                    () => RegistryPathValidator.ValidateKeyPath(path));
            Assert.AreEqual("path", ex.ParamName);
        }

        [TestMethod]
        public void ValidateRegistryKeyPath_PathWith255CharName_DoesNotThrow()
        {
            var path = $@"{_validBaseKeyName}\{new string(Enumerable.Repeat('a', 255).ToArray())}";
            RegistryPathValidator.ValidateKeyPath(path);
        }

        [TestMethod]
        public void ValidateRegistryKeyPath_MoreThan32724CharacterPath_Throws()
        {
            var totalLength = 32725;
            var path = BuildRegistryPath(_validBaseKeyName, totalLength);
            var ex =
                Assert.ThrowsException<ArgumentException>(
                    () => RegistryPathValidator.ValidateKeyPath(path));
            Assert.AreEqual("path", ex.ParamName);
        }

        [TestMethod]
        public void ValidateRegistryKeyPath_32724CharacterPath_DoesNotThrow()
        {
            var totalLength = 32724;
            var path = BuildRegistryPath(_validBaseKeyName, totalLength);
            RegistryPathValidator.ValidateKeyPath(path);
        }

        private string BuildRegistryPath(string baseKey, int totalLength)
        {
            var path = baseKey + "\\";
            while (path.Length + 256 < totalLength)
                path += new string(Enumerable.Repeat('a', 255).ToArray()) + @"\";
            var lastPathLength = totalLength - path.Length;
            return path + new string(Enumerable.Repeat('a', lastPathLength).ToArray());
        }
    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bonsai.IO;
using System.IO;

namespace Bonsai.System.Tests
{
    [TestClass]
    public class PathHelperTests
    {
        const string BasePath = "Folder\\Subfolder";
        const string Extension = ".txt";
        const string Suffix = "_suf";

        [TestMethod]
        public void AppendSuffix_EmptyPathEmptySuffix_ReturnsEmptyPath()
        {
            var path = PathHelper.AppendSuffix(string.Empty, string.Empty);
            Assert.AreEqual(string.Empty, path);
        }

        [TestMethod]
        public void AppendSuffix_EmptyPath_ReturnsSuffix()
        {
            var path = PathHelper.AppendSuffix(string.Empty, Suffix);
            Assert.AreEqual(Suffix, path);
        }

        [TestMethod]
        public void AppendSuffix_EmptySuffix_ReturnsPath()
        {
            var path = PathHelper.AppendSuffix(BasePath, string.Empty);
            Assert.AreEqual(BasePath, path);
        }

        [TestMethod]
        public void AppendSuffix_PathWithoutExtension_ReturnsPathAndSuffix()
        {
            var path = PathHelper.AppendSuffix(BasePath, Suffix);
            Assert.AreEqual(BasePath + Suffix, path);
        }

        [TestMethod]
        public void AppendSuffix_ExtensionWithEmptyFileName_ReturnsSuffixAndExtension()
        {
            var path = PathHelper.AppendSuffix(Extension, Suffix);
            Assert.AreEqual(Suffix + Extension, path);
        }

        [TestMethod]
        public void AppendSuffix_PathWithExtension_ReturnsSuffixBeforeExtension()
        {
            var basePath = Path.ChangeExtension(BasePath, Extension);
            var path = PathHelper.AppendSuffix(basePath, Suffix);
            Assert.AreEqual(Path.GetExtension(basePath), Path.GetExtension(path));
            Assert.AreEqual(Path.GetFileNameWithoutExtension(basePath) + Suffix,
                            Path.GetFileNameWithoutExtension(path));
        }
    }
}

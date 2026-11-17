using System;
using System.IO;
using System.Linq;
using Bonsai.Build;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonsai.Tests;

[TestClass]
public sealed class WindowsDesktopFrameworkTests
{
    const string WindowsDesktopFramework = "Microsoft.WindowsDesktop.App";

    static bool IsWindowsDesktop(string path) => path.Contains(WindowsDesktopFramework, StringComparison.OrdinalIgnoreCase);

    [TestMethod]
    public void GetReferencePaths_SystemDrawing_ExcludesWindowsDesktop()
    {
        var paths = FrameworkReferences.GetReferencePaths(["System", "System.Drawing"]).ToArray();
        Assert.IsFalse(paths.Any(IsWindowsDesktop));
        Assert.AreEqual(1, paths.Count(path => Path.GetFileName(path) == "System.Drawing.dll"));
    }

    [TestMethod]
    public void GetReferencePaths_SystemWindowsForms_IncludesWindowsDesktop()
    {
        var paths = FrameworkReferences.GetReferencePaths(["System", "System.Windows.Forms"]).ToArray();
        Assert.IsTrue(paths.Any(path => IsWindowsDesktop(path) && Path.GetFileName(path) == "System.Windows.Forms.dll"));
        Assert.AreEqual(1, paths.Count(path => Path.GetFileName(path) == "System.Drawing.dll"));
    }
}

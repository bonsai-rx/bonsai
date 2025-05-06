using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonsai.Editor.Tests
{
    [TestClass]
    [DoNotParallelize] // Tests involve filesystem at current working directory and must not be ran concurrently
    public class WorkflowExporterTests
    {
        static void ExportImage(string fileName)
        {
            EditorHelper.SaveEmbeddedResource(fileName, fileName);
            try
            {
                WorkflowExporter.ExportImage(fileName, Path.ChangeExtension(fileName, ".svg"));
            }
            finally { File.Delete(fileName); }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ExportImage_WorkflowContainsMissingTypes_InvalidOperationException()
        {
            ExportImage("MissingTypes.bonsai");
        }

        [TestMethod]
        public void ExportImage_WorkflowContainsMissingSubjects_ExportSuccessful()
        {
            // Skip test if running on Mono since there is no built-in ability to run headless GDI calls
            if (EditorSettings.IsRunningOnMono)
                Assert.Inconclusive();

            ExportImage("MissingSubject.bonsai");
        }
    }
}

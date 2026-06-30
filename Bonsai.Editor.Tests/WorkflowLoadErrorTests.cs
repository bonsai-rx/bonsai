using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonsai.Editor.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class WorkflowLoadErrorTests
    {
        static string CreateTempWorkflowFile(string contents)
        {
            var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".bonsai");
            File.WriteAllText(path, contents);
            return path;
        }

        [TestMethod]
        public void Run_NonXmlContent_ThrowsInvalidFormatWithInner()
        {
            var path = CreateTempWorkflowFile("this is not a workflow at all");
            try
            {
                var exception = Assert.ThrowsException<InvalidOperationException>(
                    () => WorkflowRunner.Run(path, new(), visualizerProvider: null));
                Assert.IsNotNull(exception.InnerException);
                Assert.IsTrue(WorkflowRunner.IsWorkflowFormatError(exception.InnerException));
                StringAssert.Contains(exception.Message, Path.GetFileName(path));
            }
            finally { File.Delete(path); }
        }

        [TestMethod]
        public void Run_WellFormedXmlWrongSchema_IsNotFormatError()
        {
            var path = CreateTempWorkflowFile("<NotAWorkflow><Element /></NotAWorkflow>");
            try
            {
                var exception = Assert.ThrowsException<InvalidOperationException>(
                    () => WorkflowRunner.Run(path, new(), visualizerProvider: null));
                Assert.IsFalse(WorkflowRunner.IsWorkflowFormatError(exception));
            }
            finally { File.Delete(path); }
        }

        [TestMethod]
        public void Run_MissingFile_ThrowsArgumentExceptionWithFileName()
        {
            var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".bonsai");
            var exception = Assert.ThrowsException<ArgumentException>(
                () => WorkflowRunner.Run(path, new(), visualizerProvider: null));
            StringAssert.Contains(exception.Message, Path.GetFileName(path));
        }
    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonsai.Editor.Tests
{
    [TestClass]
    public class WordSeparationTests
    {
        [DataTestMethod]
        [DataRow("")]
        [DataRow("Point.X", "Point.", "X")]
        [DataRow("State Space", "State ", "Space")]
        [DataRow("TimeStep.ElapsedTime", "Time", "Step.", "Elapsed", "Time")]
        [DataRow("UpdateVRState", "Update", "VR", "State")]
        [DataRow("Get-DisplayLED", "Get-", "Display", "LED")]
        [DataRow("A_LARGE_NAME", "A_", "LARGE_", "NAME")]
        [DataRow("Get-LatestI2CReading", "Get-", "Latest", "I2C", "Reading")]
        [DataRow("ObserveOnTaskPool", "Observe", "On", "Task", "Pool")]
        public void SplitOnWordBoundaries_ExpectedWordSequence(string text, params string[] expectedWords)
        {
            var words = text.SplitOnWordBoundaries();
            CollectionAssert.AreEqual(expectedWords, words);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SplitOnWordBoundaries_NullString_ThrowsArgumentNullException()
        {
            StringExtensions.SplitOnWordBoundaries(null);
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonsai.Core.Tests
{
    [TestClass]
    public class RangeTests
    {
        readonly Range<int> Single = Range.Create(1, 1);
        readonly Range<int> ZeroOrOne = Range.Create(0, 1);

        [TestMethod]
        public void IEquatable_EqualValues_ReturnTrue()
        {
            EquatableTests.AssertEquatable(Single, Single, true);
        }

        [TestMethod]
        public void IEquatable_DifferentValues_ReturnFalse()
        {
            EquatableTests.AssertEquatable(Single, ZeroOrOne, false);
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Core.Tests
{
    [TestClass]
    public class ExpressionHelperTests
    {
        static readonly Expression NullConstant = Expression.Constant(null);
        static readonly Expression PrimitiveConstant = Expression.Constant(string.Empty);
        static readonly Type PrimitiveAccessMemberType = typeof(int);
        const string PrimitiveAccess = "Length";

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MemberAccess_NullInstance_ThrowsArgumentNullException()
        {
            ExpressionHelper.MemberAccess(null, string.Empty);
        }

        [TestMethod]
        public void MemberAccess_NullMemberPath_ReturnsInstance()
        {
            var result = ExpressionHelper.MemberAccess(PrimitiveConstant, null);
            Assert.AreSame(PrimitiveConstant, result);
        }

        [TestMethod]
        public void MemberAccess_EmptyMemberPath_ReturnsInstance()
        {
            var result = ExpressionHelper.MemberAccess(PrimitiveConstant, string.Empty);
            Assert.AreSame(PrimitiveConstant, result);
        }

        [TestMethod]
        public void MemberAccess_SelfMemberPath_ReturnsInstance()
        {
            var result = ExpressionHelper.MemberAccess(PrimitiveConstant, ExpressionHelper.ImplicitParameterName);
            Assert.AreSame(PrimitiveConstant, result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void MemberAccess_DoubleSeparator_ThrowsArgumentException()
        {
            ExpressionHelper.MemberAccess(PrimitiveConstant, "..");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void MemberAccess_PrefixSeparator_ThrowsArgumentException()
        {
            ExpressionHelper.MemberAccess(PrimitiveConstant, "." + PrimitiveAccess);
        }

        [TestMethod]
        public void MemberAccess_ExistingMember_ReturnsMemberExpression()
        {
            var result = ExpressionHelper.MemberAccess(PrimitiveConstant, PrimitiveAccess);
            Assert.AreEqual(PrimitiveAccessMemberType, result.Type);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void MemberAccess_MethodName_ThrowsArgumentException()
        {
            ExpressionHelper.MemberAccess(PrimitiveConstant, "ToString");
        }
    }
}

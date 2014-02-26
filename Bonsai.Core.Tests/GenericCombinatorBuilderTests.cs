using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq.Expressions;
using Bonsai.Expressions;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Bonsai.Core.Tests
{
    public partial class CombinatorBuilderTests
    {
        [Combinator]
        class GenericCombinatorMock
        {
            public IObservable<T> Process<T>(IObservable<T> source)
            {
                return source;
            }
        }

        [TestMethod]
        public void Build_GenericProcessMethod_ReturnsValue()
        {
            var value = 5;
            var combinator = new GenericCombinatorMock();
            var source = CreateObservableExpression(Observable.Return(value));
            var resultProvider = TestCombinatorBuilder<int>(combinator, source);
            var result = Last(resultProvider).Result;
            Assert.AreEqual(value, result);
        }
    }
}

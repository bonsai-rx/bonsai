using System;
using System.Reactive.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonsai.Core.Tests
{
    public partial class CombinatorBuilderTests
    {
        [Combinator]
        class TypeCombinatorMock<T> : Combinator<T, T>
        {
            public override IObservable<T> Process(IObservable<T> source)
            {
                return source;
            }
        }

        private void BuildConvertCast<TSource, TResult>(TSource value, out TResult result)
        {
            var combinator = new TypeCombinatorMock<TResult>();
            var source = CreateObservableExpression(Observable.Return(value));
            var resultProvider = TestCombinatorBuilder<TResult>(combinator, source);
            result = Last(resultProvider).Result;
        }

        [TestMethod]
        public void Build_ConvertDoubleToInt_ReturnsTruncatedValue()
        {
            var value = 5.5;
            BuildConvertCast(value, out int result);
            Assert.AreEqual((int)value, result);
        }

        [TestMethod]
        public void Build_ConvertObjectToImplementedInterface_ReturnsValidObject()
        {
            BuildConvertCast(5.5, out IComparable result);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(WorkflowBuildException))]
        public void Build_ConvertObjectToNotImplementedInterface_ThrowsBuildException()
        {
            BuildConvertCast(5.5, out IDisposable result);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(WorkflowBuildException))]
        public void Build_ConvertInterfaceToDifferentInterface_ThrowsBuildException()
        {
            BuildConvertCast(5.5, out IComparable result);
            BuildConvertCast(result, out IDisposable disposable);
            Assert.AreSame((IDisposable)result, disposable);
        }
    }
}

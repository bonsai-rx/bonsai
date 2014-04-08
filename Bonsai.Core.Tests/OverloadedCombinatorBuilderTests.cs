using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq.Expressions;
using Bonsai.Expressions;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Bonsai.Core.Tests
{
    public partial class CombinatorBuilderTests
    {
        [Combinator]
        class OverloadedCombinatorMock
        {
            public IObservable<float> Process(IObservable<float> source)
            {
                return source;
            }

            public IObservable<double> Process(IObservable<double> source)
            {
                return source;
            }
        }

        [Combinator]
        class ParamsOverloadedCombinatorMock
        {
            public IObservable<float> Process(params IObservable<float>[] source)
            {
                return source.FirstOrDefault();
            }

            public IObservable<double> Process(params IObservable<double>[] source)
            {
                return source.FirstOrDefault();
            }
        }

        [Combinator]
        class ListTupleOverloadedCombinatorMock
        {
            public IObservable<int> Process(IObservable<Tuple<int, int>> source)
            {
                return source.Select(xs => xs.Item1);
            }

            public IObservable<IList<int>> Process(IObservable<IList<int>> source)
            {
                return source;
            }
        }

        [TestMethod]
        public void Build_DoubleOverloadedMethodCalledWithDouble_ReturnsDoubleValue()
        {
            var value = 5.0;
            var combinator = new OverloadedCombinatorMock();
            var source = CreateObservableExpression(Observable.Return(value));
            var resultProvider = TestCombinatorBuilder<double>(combinator, source);
            var result = Last(resultProvider).Result;
            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public void Build_FloatOverloadedMethodCalledWithInt_ReturnsFloatValue()
        {
            var value = 5;
            var combinator = new OverloadedCombinatorMock();
            var source = CreateObservableExpression(Observable.Return(value));
            var resultProvider = TestCombinatorBuilder<float>(combinator, source);
            var result = Last(resultProvider).Result;
            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public void Build_ParamsFloatOverloadedMethodCalledWithInt_ReturnsFloatValue()
        {
            var value = 5;
            var combinator = new ParamsOverloadedCombinatorMock();
            var source = CreateObservableExpression(Observable.Return(value));
            var resultProvider = TestCombinatorBuilder<float>(combinator, source);
            var result = Last(resultProvider).Result;
            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public void Build_ListTupleOverloadedMethodCalledWithIntTuple_ReturnsIntValue()
        {
            var value = 5;
            var combinator = new ListTupleOverloadedCombinatorMock();
            var source = CreateObservableExpression(Observable.Return(Tuple.Create(value, value)));
            var resultProvider = TestCombinatorBuilder<int>(combinator, source);
            var result = Last(resultProvider).Result;
            Assert.AreEqual(value, result);
        }
    }
}

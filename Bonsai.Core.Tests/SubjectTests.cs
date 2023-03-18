using System;
using System.Reactive.Linq;
using Bonsai.Expressions;
using Bonsai.Reactive;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonsai.Core.Tests
{
    [TestClass]
    public class SubjectTests
    {
        [TestMethod]
        public void ResourceSubject_SourceTerminatesExceptionally_ShouldNotTryToDispose()
        {
            var workflowBuilder = new WorkflowBuilder();
            var source = workflowBuilder.Workflow.Add(new CombinatorBuilder { Combinator = new ThrowSource() });
            var subject = workflowBuilder.Workflow.Add(new ResourceSubject { Name = nameof(ResourceSubject) });
            var sink = workflowBuilder.Workflow.Add(new CombinatorBuilder { Combinator = new CatchSink() });
            workflowBuilder.Workflow.AddEdge(source, subject, new ExpressionBuilderArgument());
            workflowBuilder.Workflow.AddEdge(subject, sink, new ExpressionBuilderArgument());
            var observable = workflowBuilder.Workflow.BuildObservable();
            observable.FirstOrDefaultAsync().Wait();
        }

        class ThrowSource : Source<IDisposable>
        {
            public override IObservable<IDisposable> Generate()
            {
                return Observable.Throw<IDisposable>(new SubjectException());
            }
        }

        class CatchSink : Sink
        {
            public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
            {
                return source.Catch<TSource>(Observable.Empty<TSource>());
            }
        }

        class SubjectException : Exception
        {
        }
    }
}

using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.IO
{
    [WorkflowElementCategory(ElementCategory.Source)]
    [Description("Sources individual lines of a text file as an observable sequence.")]
    public class CsvReader : CombinatorExpressionBuilder
    {
        public CsvReader()
            : base(minArguments: 0, maxArguments: 0)
        {
        }

        [Description("The name of the CSV file.")]
        [FileNameFilter("CSV (Comma delimited)|*.csv|All Files|*.*")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string FileName { get; set; }

        [TypeConverter("Bonsai.Expressions.ParseBuilder+PatternConverter, Bonsai.Core")]
        [Description("The parse pattern for scanning individual lines, including conversion specifications for output data types.")]
        public string ScanPattern { get; set; }

        protected override Expression BuildCombinator(IEnumerable<Expression> arguments)
        {
            var scanPattern = ScanPattern;
            var parameter = Expression.Parameter(typeof(string));
            var parseBody = !string.IsNullOrEmpty(scanPattern) ? ExpressionHelper.Parse(parameter, scanPattern) : parameter;
            var parser = Expression.Lambda(parseBody, parameter);
            var combinatorExpression = Expression.Constant(this);
            return Expression.Call(combinatorExpression, "Generate", new[] { parseBody.Type }, parser);
        }

        IObservable<TResult> Generate<TResult>(Func<string, TResult> parser)
        {
            return Observable.Create<TResult>(async (observer, token) =>
            {
                try
                {
                    using (var reader = new StreamReader(FileName))
                    {
                        while (!reader.EndOfStream)
                        {
                            if (token.IsCancellationRequested) break;
                            var line = await reader.ReadLineAsync();
                            observer.OnNext(parser(line));
                        }
                    }

                    observer.OnCompleted();
                }
                catch (Exception ex)
                {
                    observer.OnError(ex);
                }
            });
        }
    }
}

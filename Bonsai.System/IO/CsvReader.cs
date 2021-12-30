using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bonsai.IO
{
    [DefaultProperty("FileName")]
    [WorkflowElementCategory(ElementCategory.Source)]
    [Description("Sources individual lines of a text file as an observable sequence.")]
    public class CsvReader : CombinatorExpressionBuilder
    {
        static readonly string[] EmptySeparator = new string[0];

        public CsvReader()
            : base(minArguments: 0, maxArguments: 1)
        {
        }

        [Description("The name of the CSV file.")]
        [FileNameFilter("CSV (Comma delimited)|*.csv|All Files|*.*")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string FileName { get; set; }

        [Description("The separator used to delimit elements in variable length rows. This argument is optional.")]
        public string ListSeparator { get; set; }

        [TypeConverter("Bonsai.Expressions.ParseBuilder+PatternConverter, Bonsai.Core")]
        [Editor("Bonsai.Design.ParsePatternEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [Description("The optional parse pattern for scanning individual lines. In case of variable length rows, the pattern will be applied to each individual element.")]
        public string ScanPattern { get; set; }

        [Description("The number of lines to skip at the start of the file.")]
        public int SkipRows { get; set; }

        protected override Expression BuildCombinator(IEnumerable<Expression> arguments)
        {
            Expression parseBody;
            var pattern = ScanPattern;
            var separatorString = ListSeparator;
            if (string.IsNullOrEmpty(pattern)) pattern = null;
            var separator = string.IsNullOrEmpty(separatorString) ? EmptySeparator : new[] { Regex.Unescape(separatorString) };

            var source = arguments.FirstOrDefault();
            var parameter = Expression.Parameter(typeof(string));
            parseBody = ExpressionHelper.Parse(parameter, pattern, separator);
            var parser = Expression.Lambda(parseBody, parameter);
            var combinatorExpression = Expression.Constant(this);
            if (source != null)
            {
                var sourceType = source.Type.GetGenericArguments()[0];
                return Expression.Call(combinatorExpression, nameof(Generate), new[] { sourceType, parseBody.Type }, source, parser);
            }
            else return Expression.Call(combinatorExpression, nameof(Generate), new[] { parseBody.Type }, parser);
        }

        IObservable<TResult> Generate<TResult>(Func<string, TResult> parser)
        {
            return Observable.Create<TResult>(async (observer, token) =>
            {
                try
                {
                    var skipRows = SkipRows;
                    using (var stream = File.OpenRead(FileName))
                    using (var reader = new StreamReader(stream))
                    {
                        while (!reader.EndOfStream)
                        {
                            if (token.IsCancellationRequested) break;
                            var line = await reader.ReadLineAsync();
                            if (skipRows > 0)
                            {
                                skipRows--;
                                continue;
                            }

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

        IObservable<TResult> Generate<TSource, TResult>(IObservable<TSource> source, Func<string, TResult> parser)
        {
            return Observable.Using(
                async token =>
                {
                    var skipRows = SkipRows;
                    var reader = new StreamReader(FileName);
                    while (!reader.EndOfStream && skipRows > 0)
                    {
                        if (token.IsCancellationRequested) break;
                        await reader.ReadLineAsync();
                        skipRows--;
                    }
                    return reader;
                },
                (reader, token) =>
                {
                    if (token.IsCancellationRequested)
                    {
                        return Task.FromResult(Observable.Empty<TResult>());
                    }

                    return Task.FromResult(source.TakeWhile(input => !reader.EndOfStream).Select(input =>
                    {
                        var line = reader.ReadLine();
                        return parser(line);
                    }));
                });
        }
    }
}

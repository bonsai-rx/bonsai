﻿using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bonsai.IO
{
    [DefaultProperty("FileName")]
    [WorkflowElementCategory(ElementCategory.Source)]
    [Description("Sources individual lines of a text file as an observable sequence.")]
    public class CsvReader : CombinatorExpressionBuilder
    {
        static readonly MethodInfo convertAllMethod = typeof(Array).GetMethod("ConvertAll");
        static readonly MethodInfo splitMethod = (from method in typeof(string).GetMethods()
                                                  where method.Name == "Split"
                                                  let parameters = method.GetParameters()
                                                  where parameters.Length == 2 && parameters[0].ParameterType == typeof(string[])
                                                  select method).Single();

        public CsvReader()
            : base(minArguments: 0, maxArguments: 0)
        {
        }

        [Description("The name of the CSV file.")]
        [FileNameFilter("CSV (Comma delimited)|*.csv|All Files|*.*")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string FileName { get; set; }

        [Description("The separator used to delimit elements in variable length rows. This argument is optional.")]
        public string ListSeparator { get; set; }

        [TypeConverter("Bonsai.Expressions.ParseBuilder+PatternConverter, Bonsai.Core")]
        [Description("The optional parse pattern for scanning individual lines. In case of variable length rows, the pattern will be applied to each individual element.")]
        public string ScanPattern { get; set; }

        [Description("The number of lines to skip at the start of the file.")]
        public int SkipRows { get; set; }

        static Expression CreateParser(ParameterExpression parameter, string scanPattern)
        {
            return !string.IsNullOrEmpty(scanPattern) ? ExpressionHelper.Parse(parameter, scanPattern) : parameter;
        }

        protected override Expression BuildCombinator(IEnumerable<Expression> arguments)
        {
            Expression parseBody;
            var separator = ListSeparator;
            var scanPattern = ScanPattern;
            var parameter = Expression.Parameter(typeof(string));
            if (!string.IsNullOrEmpty(separator))
            {
                var separatorExpression = Expression.Constant(Regex.Unescape(separator));
                var splitOptions = Expression.Constant(StringSplitOptions.RemoveEmptyEntries);
                var columns = Expression.Call(parameter, splitMethod, Expression.NewArrayInit(typeof(string), separatorExpression), splitOptions);
                var columnParameter = Expression.Parameter(typeof(string));
                var columnParseBody = CreateParser(columnParameter, scanPattern);
                var columnConverterType = typeof(Converter<,>).MakeGenericType(columnParameter.Type, columnParseBody.Type);
                var columnParser = Expression.Lambda(columnConverterType, columnParseBody, columnParameter);
                parseBody = Expression.Call(convertAllMethod.MakeGenericMethod(columnParameter.Type, columnParseBody.Type), columns, columnParser);
            }
            else parseBody = CreateParser(parameter, scanPattern);
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
                    var skipRows = SkipRows;
                    using (var reader = new StreamReader(FileName))
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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Globalization;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using Bonsai.Expressions;
using System.Text.RegularExpressions;

namespace Bonsai.IO
{
    /// <summary>
    /// Represents an operator that writes a delimited text representation of each element
    /// of the sequence to a text file.
    /// </summary>
    [DefaultProperty(nameof(FileName))]
    [WorkflowElementCategory(ElementCategory.Sink)]
    [Description("Writes a delimited text representation of each element of the sequence to a text file.")]
    public class CsvWriter : CombinatorExpressionBuilder
    {
        string delimiter;
        static readonly Expression InvariantCulture = Expression.Constant(CultureInfo.InvariantCulture);
        static readonly MethodInfo stringJoinMethod = typeof(string).GetMethods().Single(m => m.Name == nameof(string.Join) &&
                                                                                             m.GetParameters().Length == 2 &&
                                                                                             m.GetParameters()[0].ParameterType == typeof(string) &&
                                                                                             m.GetParameters()[1].ParameterType == typeof(string[]));
        static readonly MethodInfo writeLineMethod = typeof(StreamWriter).GetMethods().Single(m => m.Name == nameof(StreamWriter.WriteLine) &&
                                                                                                  m.GetParameters().Length == 1 &&
                                                                                                  m.GetParameters()[0].ParameterType == typeof(string));

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvWriter"/> class.
        /// </summary>
        public CsvWriter()
            : base(minArguments: 1, maxArguments: 1)
        {
        }
        
        /// <summary>
        /// Gets or sets the name of the output CSV file.
        /// </summary>
        [Description("The name of the output CSV file.")]
        [FileNameFilter("CSV (Comma delimited)|*.csv|All Files|*.*")]
        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the optional delimiter used to separate columns in the output file.
        /// </summary>
        [DefaultValue("")]
        [Description("The optional delimiter used to separate columns in the output file.")]
        public string Delimiter
        {
            get { return delimiter; }
            set
            {
                delimiter = value;
#pragma warning disable CS0612 // Type or member is obsolete
                CompatibilityMode = false;
#pragma warning restore CS0612 // Type or member is obsolete
            }
        }

        /// <summary>
        /// Gets or sets the separator used to delimit elements in variable length rows. This argument is optional.
        /// </summary>
        [Description("The separator used to delimit elements in variable length rows. This argument is optional.")]
        public string ListSeparator { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether data should be appended to the output file if it already exists.
        /// </summary>
        [Description("Indicates whether data should be appended to the output file if it already exists.")]
        public bool Append { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the output file should be overwritten if it already exists.
        /// </summary>
        [Description("Indicates whether the output file should be overwritten if it already exists.")]
        public bool Overwrite { get; set; }

        /// <summary>
        /// Gets or sets the suffix used to generate file names.
        /// </summary>
        [Description("The suffix used to generate file names.")]
        public PathSuffix Suffix { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include a text header with column names for multi-attribute values.
        /// </summary>
        [Description("Indicates whether to include a text header with column names for multi-attribute values.")]
        public bool IncludeHeader { get; set; }

        /// <summary>
        /// Gets or sets the inner properties that will be selected when writing each element of the sequence.
        /// </summary>
        [Description("The inner properties that will be selected when writing each element of the sequence.")]
        [Editor("Bonsai.IO.Design.DataMemberSelectorEditor, Bonsai.System.Design", DesignTypes.UITypeEditor)]
        public string Selector { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the serialized <see cref="CsvWriter"/> should use
        /// white space rather than commas as default delimiter.
        /// </summary>
        [Obsolete]
        [Browsable(false)]
        [DefaultValue(false)]
        public bool CompatibilityMode { get; set; }

        class ExpressionNode
        {
            public Expression Expression { get; set; }
            public ExpressionNode Parent { get; set; }

            public IEnumerable<Expression> GetPath()
            {
                var current = this;
                while (current != null)
                {
                    yield return current.Expression;
                    current = current.Parent;
                }
            }
        }

        static IEnumerable<Expression> MakeMemberAccess(IEnumerable<Expression> expressions)
        {
            var stack = new Stack<ExpressionNode>();
            foreach (var expression in expressions)
            {
                stack.Push(new ExpressionNode { Expression = expression });
            }

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                var expression = current.Expression;
                if (expression.Type == typeof(string) || IsNullable(expression.Type)) yield return expression;
                else if (expression.Type.IsPrimitive || expression.Type.IsEnum || expression.Type == typeof(string) ||
                         expression.Type == typeof(DateTime) || expression.Type == typeof(DateTimeOffset) ||
                         expression.Type == typeof(TimeSpan))
                {
                    yield return expression;
                }
                else
                {
                    var successors = from member in GetDataMembers(expression.Type)
                                     let memberAccess = (Expression)Expression.MakeMemberAccess(expression, member)
                                     select memberAccess;
                    foreach (var successor in successors)
                    {
                        if (current.GetPath().Any(node => node.Type == successor.Type)) continue;
                        stack.Push(new ExpressionNode { Expression = successor, Parent = current });
                    }
                }
            }
        }

        static IEnumerable<MemberInfo> GetDataMembers(Type type)
        {
            var members = Enumerable.Concat<MemberInfo>(
                type.GetFields(BindingFlags.Instance | BindingFlags.Public),
                type.GetProperties(BindingFlags.Instance | BindingFlags.Public));
            if (type.IsInterface)
            {
                members = members.Concat(type
                    .GetInterfaces()
                    .SelectMany(i => i.GetProperties(BindingFlags.Instance | BindingFlags.Public)));
            }
            return members.Where(member => !member.IsDefined(typeof(XmlIgnoreAttribute), true))
                          .OrderBy(member => member.MetadataToken)
                          .Except(type.GetDefaultMembers());
        }

        static bool IsNullable(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        static Expression GetDataString(Expression expression)
        {
            if (expression.Type == typeof(string)) return expression;
            if (expression.Type == typeof(float))
            {
                return Expression.Call(expression, nameof(ToString), null, Expression.Constant("G9"), InvariantCulture);
            }
            else if (expression.Type == typeof(double))
            {
                return Expression.Call(expression, nameof(ToString), null, Expression.Constant("G17"), InvariantCulture);
            }
            else if (expression.Type == typeof(DateTime) || expression.Type == typeof(DateTimeOffset))
            {
                return Expression.Call(expression, nameof(ToString), null, Expression.Constant("o"));
            }
            else if (expression.Type == typeof(IntPtr) || expression.Type == typeof(TimeSpan))
            {
                return Expression.Call(expression, nameof(ToString), null);
            }
            else if (IsNullable(expression.Type))
            {
                var hasValue = Expression.Property(expression, nameof(Nullable<int>.HasValue));
                var value = Expression.Property(expression, nameof(Nullable<int>.Value));
                return Expression.Condition(hasValue, GetDataString(value), Expression.Constant(string.Empty));
            }
            else
            {
                return Expression.Call(expression, nameof(ToString), null, InvariantCulture);
            }
        }

        /// <inheritdoc/>
        protected override Expression BuildCombinator(IEnumerable<Expression> arguments)
        {
            const string InputParameterName = "input";
            const string DataParameterName = "item";

            var source = arguments.First();
            var parameterType = source.Type.GetGenericArguments()[0];
            var inputParameter = Expression.Parameter(parameterType, InputParameterName);
            var dataType = ExpressionHelper.GetGenericTypeBindings(typeof(IList<>), parameterType).FirstOrDefault() ?? parameterType;
            var dataParameter = dataType == parameterType ? inputParameter : Expression.Parameter(dataType, DataParameterName);
            var writerParameter = Expression.Parameter(typeof(StreamWriter));
            var selectedMembers = ExpressionHelper.SelectMembers(dataParameter, Selector);

            var delimiter = Delimiter;
            if (string.IsNullOrEmpty(delimiter))
            {
#pragma warning disable CS0612 // Type or member is obsolete
                delimiter = CompatibilityMode ? " " : CultureInfo.InvariantCulture.TextInfo.ListSeparator;
#pragma warning restore CS0612 // Type or member is obsolete
            }

            var listSeparator = ListSeparator;
            if (string.IsNullOrEmpty(listSeparator))
            {
                listSeparator = delimiter;
            }

            var delimiterConstant = Expression.Constant(Regex.Unescape(delimiter));
            var listSeparatorConstant = listSeparator == delimiter
                ? delimiterConstant
                : Expression.Constant(Regex.Unescape(listSeparator));

#pragma warning disable CS0612 // Type or member is obsolete
            var legacyCharacter = CompatibilityMode
                ? Enumerable.Repeat(Expression.Constant(string.Empty), 1)
                : Enumerable.Empty<Expression>();
#pragma warning restore CS0612 // Type or member is obsolete
            var memberAccessExpressions = legacyCharacter.Concat(MakeMemberAccess(selectedMembers))
                .Select(memberAccess => GetDataString(memberAccess))
                .ToArray();
            Array.Reverse(memberAccessExpressions);

            var memberAccessArrayExpression = Expression.NewArrayInit(typeof(string), memberAccessExpressions);
            var lineExpression = Expression.Call(stringJoinMethod, delimiterConstant, memberAccessArrayExpression);
            if (dataParameter != inputParameter)
            {
                var converterExpression = Expression.Lambda(lineExpression, dataParameter);
                lineExpression = Expression.Call(
                    typeof(CsvWriter),
                    nameof(ListJoin),
                    new[] { dataType },
                    inputParameter,
                    converterExpression,
                    listSeparatorConstant);
            }
            var body = Expression.Call(writerParameter, writeLineMethod, lineExpression);
            var writeAction = Expression.Lambda(body, inputParameter, writerParameter);

            var header = string.Empty;
            if (IncludeHeader)
            {
                var headerMembers = memberAccessExpressions.Select(memberAccess =>
                {
                    var accessExpression = memberAccess.ToString();
                    var firstAccess = accessExpression.IndexOf(ExpressionHelper.MemberSeparator);
                    if (firstAccess++ < 0) return string.Empty;
                    var lastAccess = accessExpression.LastIndexOf(ExpressionHelper.MemberSeparator);
                    if (lastAccess < firstAccess) lastAccess = accessExpression.Length;
                    return accessExpression.Substring(firstAccess, lastAccess - firstAccess);
                }).Where(access => !string.IsNullOrEmpty(access)).ToArray();
                header = string.Join((string)delimiterConstant.Value, headerMembers);
                header = header.Trim();
            }

            var csvWriter = Expression.Constant(this);
            var headerExpression = Expression.Constant(header);
            return Expression.Call(csvWriter, nameof(Process), new[] { parameterType }, source, headerExpression, writeAction);
        }

        static string ListJoin<TSource>(IList<TSource> source, Func<TSource, string> converter, string separator)
        {
            return string.Join(separator, source.Select(converter));
        }

        IObservable<TSource> Process<TSource>(IObservable<TSource> source, string header, Action<TSource, StreamWriter> writeAction)
        {
            return Observable.Create<TSource>(observer =>
            {
                var fileName = FileName;
                if (string.IsNullOrEmpty(fileName))
                {
                    throw new InvalidOperationException("A valid file path must be specified.");
                }

                PathHelper.EnsureDirectory(fileName);
                fileName = PathHelper.AppendSuffix(fileName, Suffix);
                if (File.Exists(fileName) && !Overwrite && !Append)
                {
                    throw new IOException(string.Format("The file '{0}' already exists.", fileName));
                }

                var disposable = new WriterDisposable<StreamWriter>();
                disposable.Schedule(() =>
                {
                    try
                    {
                        var writer = new StreamWriter(fileName, Append, Encoding.ASCII);
                        if (!string.IsNullOrEmpty(header)) writer.WriteLine(header);
                        disposable.Writer = writer;
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }
                });

                var process = source.Do(input =>
                {
                    disposable.Schedule(() =>
                    {
                        try { writeAction(input, disposable.Writer); }
                        catch (Exception ex)
                        {
                            observer.OnError(ex);
                        }
                    });
                }).SubscribeSafe(observer);

                return new CompositeDisposable(process, disposable);
            });
        }
    }
}

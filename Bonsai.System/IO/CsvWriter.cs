using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Globalization;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using Bonsai.Expressions;

namespace Bonsai.IO
{
    [DefaultProperty("FileName")]
    [WorkflowElementCategory(ElementCategory.Sink)]
    [Description("Sinks individual elements of the input sequence to a text file.")]
    public class CsvWriter : CombinatorExpressionBuilder
    {
        string delimiter;
        static readonly MethodInfo stringJoinMethod = typeof(string).GetMethods().Single(m => m.Name == "Join" &&
                                                                                             m.GetParameters().Length == 2 &&
                                                                                             m.GetParameters()[1].ParameterType == typeof(string[]));
        static readonly MethodInfo writeLineMethod = typeof(StreamWriter).GetMethods().Single(m => m.Name == "WriteLine" &&
                                                                                                  m.GetParameters().Length == 1 &&
                                                                                                  m.GetParameters()[0].ParameterType == typeof(string));

        public CsvWriter()
            : base(minArguments: 1, maxArguments: 1)
        {
        }
        
        [Description("The name of the output file.")]
        [FileNameFilter("CSV (Comma delimited)|*.csv|All Files|*.*")]
        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string FileName { get; set; }

        [DefaultValue("")]
        [Description("The optional delimiter used to separate columns in the output file.")]
        public string Delimiter
        {
            get { return delimiter; }
            set
            {
                delimiter = value;
                CompatibilityMode = false;
            }
        }

        [Description("Indicates whether data should be appended to the output file if it already exists.")]
        public bool Append { get; set; }

        [Description("Indicates whether the output file should be overwritten if it already exists.")]
        public bool Overwrite { get; set; }

        [Description("The optional suffix used to generate file names.")]
        public PathSuffix Suffix { get; set; }

        [Description("Indicates whether to include a text header with column names for multi-dimensional input.")]
        public bool IncludeHeader { get; set; }

        [Description("The inner properties that will be selected for output in each element of the sequence.")]
        [Editor("Bonsai.Design.MultiMemberSelectorEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string Selector { get; set; }

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
                if (expression.Type == typeof(string)) yield return expression;
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
                          .OrderBy(member => member.MetadataToken);
        }

        static Expression GetDataString(Expression expression)
        {
            if (expression.Type == typeof(string)) return expression;
            else if (expression.Type == typeof(DateTime) || expression.Type == typeof(DateTimeOffset))
            {
                return Expression.Call(expression, "ToString", null, Expression.Constant("o"));
            }
            else if (expression.Type == typeof(IntPtr) || expression.Type == typeof(TimeSpan))
            {
                return Expression.Call(expression, "ToString", null);
            }
            else
            {
                return Expression.Call(expression, "ToString", null, Expression.Constant(CultureInfo.InvariantCulture));
            }
        }

        protected override Expression BuildCombinator(IEnumerable<Expression> arguments)
        {
            const string ParameterName = "input";

            var source = arguments.First();
            var parameterType = source.Type.GetGenericArguments()[0];
            var inputParameter = Expression.Parameter(parameterType, ParameterName);
            var writerParameter = Expression.Parameter(typeof(StreamWriter));
            var selectedMembers = SelectMembers(inputParameter, Selector);

            var delimiter = Delimiter;
            if (string.IsNullOrEmpty(delimiter))
            {
                delimiter = CompatibilityMode ? " " : CultureInfo.InvariantCulture.TextInfo.ListSeparator;
            }
            var delimiterConstant = Expression.Constant(delimiter);

            var legacyCharacter = CompatibilityMode
                ? Enumerable.Repeat(Expression.Constant(string.Empty), 1)
                : Enumerable.Empty<Expression>();
            var memberAccessExpressions = legacyCharacter.Concat(MakeMemberAccess(selectedMembers))
                .Select(memberAccess => GetDataString(memberAccess))
                .ToArray();
            Array.Reverse(memberAccessExpressions);

            var memberAccessArrayExpression = Expression.NewArrayInit(typeof(string), memberAccessExpressions);
            var lineExpression = Expression.Call(stringJoinMethod, delimiterConstant, memberAccessArrayExpression);
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
            return Expression.Call(csvWriter, "Process", new[] { parameterType }, source, headerExpression, writeAction);
        }

        IObservable<TSource> Process<TSource>(IObservable<TSource> source, string header, Action<TSource, StreamWriter> writeAction)
        {
            return Observable.Using(
                () =>
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

                    var disposable = new WriterDisposable();
                    disposable.WriterTask = new Task(() =>
                    {
                        var writer = new StreamWriter(fileName, Append, Encoding.ASCII);
                        if (!string.IsNullOrEmpty(header)) writer.WriteLine(header);
                        disposable.Writer = writer;
                    });
                    disposable.WriterTask.Start();
                    return disposable;
                },
                disposable => source.Do(input =>
                {
                    disposable.WriterTask = disposable.WriterTask.ContinueWith(task => writeAction(input, disposable.Writer));
                }));
        }

        class WriterDisposable : IDisposable
        {
            public Task WriterTask { get; set; }

            public StreamWriter Writer { get; set; }

            public void Dispose()
            {
                WriterTask.ContinueWith(task =>
                {
                    var closingWriter = Writer;
                    if (closingWriter != null)
                    {
                        closingWriter.Close();
                    }
                });
            }
        }
    }
}

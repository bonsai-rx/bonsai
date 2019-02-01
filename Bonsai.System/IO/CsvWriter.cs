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
using System.Reactive.Concurrency;

namespace Bonsai.IO
{
    [DefaultProperty("FileName")]
    [WorkflowElementCategory(ElementCategory.Sink)]
    [Description("Sinks individual elements of the input sequence to a text file.")]
    public class CsvWriter : CombinatorExpressionBuilder
    {
        static readonly MethodInfo writeLineMethod = typeof(StreamWriter).GetMethods().First(m => m.Name == "WriteLine" && m.GetParameters().Length == 0);
        static readonly MethodInfo writeMethod = typeof(StreamWriter).GetMethods().First(m => m.Name == "Write" &&
                                                                                              m.GetParameters().Length == 2 &&
                                                                                              m.GetParameters()[1].ParameterType == typeof(object));

        public CsvWriter()
            : base(minArguments: 1, maxArguments: 1)
        {
        }
        
        [Description("The name of the output file.")]
        [FileNameFilter("CSV (Comma delimited)|*.csv|All Files|*.*")]
        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string FileName { get; set; }

        [Description("Indicates whether data should be appended to the output file if it already exists.")]
        public bool Append { get; set; }

        [Description("Indicates whether the output file should be overwritten if it already exists.")]
        public bool Overwrite { get; set; }

        [Description("The optional suffix used to generate file names.")]
        public PathSuffix Suffix { get; set; }

        [Description("Indicates whether to include a text header with column names for multi-dimensional input.")]
        public bool IncludeHeader { get; set; }

        [Description("The inner properties that will be selected for output in each element of the sequence.")]
        [Editor("Bonsai.Design.MultiMemberSelectorEditor, Bonsai.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string Selector { get; set; }

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
            const string EntryFormat = "{0} ";

            var source = arguments.First();
            var parameterType = source.Type.GetGenericArguments()[0];
            var inputParameter = Expression.Parameter(parameterType, ParameterName);
            var writerParameter = Expression.Parameter(typeof(StreamWriter));
            var selectedMembers = SelectMembers(inputParameter, Selector);
            var formatConstant = Expression.Constant(EntryFormat);

            var memberAccessExpressions = MakeMemberAccess(selectedMembers).ToArray();
            Array.Reverse(memberAccessExpressions);

            var writeParameters = from memberAccess in memberAccessExpressions
                                  let memberString = GetDataString(memberAccess)
                                  select Expression.Call(writerParameter, writeMethod, formatConstant, memberString);
            var writeLineExpression = Expression.Call(writerParameter, writeLineMethod);
            var bodyExpressions = writeParameters.Concat(Enumerable.Repeat(writeLineExpression, 1)).ToArray();

            var body = Expression.Block(bodyExpressions);
            var writeAction = Expression.Lambda(body, inputParameter, writerParameter);

            var header = string.Empty;
            if (IncludeHeader)
            {
                foreach (var memberAccess in memberAccessExpressions)
                {
                    var accessExpression = memberAccess.ToString();
                    header += string.Format(
                        EntryFormat,
                        accessExpression.Remove(accessExpression.IndexOf(ParameterName), ParameterName.Length))
                        .TrimStart('.');
                }
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
                        throw new InvalidOperationException("A valid filename must be specified.");
                    }

                    PathHelper.EnsureDirectory(fileName);
                    fileName = PathHelper.AppendSuffix(fileName, Suffix);
                    if (File.Exists(fileName) && !Overwrite && !Append)
                    {
                        throw new IOException(string.Format("The file '{0}' already exists.", fileName));
                    }

                    var disposable = new WriterDisposable<StreamWriter>();
                    disposable.Scheduler.Schedule(() =>
                    {
                        var writer = new StreamWriter(fileName, Append, Encoding.ASCII);
                        if (!string.IsNullOrEmpty(header)) writer.WriteLine(header);
                        disposable.Writer = writer;
                    });
                    return disposable;
                },
                disposable => source.Do(input =>
                {
                    disposable.Scheduler.Schedule(() => writeAction(input, disposable.Writer));
                }));
        }
    }
}

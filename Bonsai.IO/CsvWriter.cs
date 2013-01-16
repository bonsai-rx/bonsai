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

namespace Bonsai.IO
{
    [Description("Sinks individual elements of the input sequence to a text file.")]
    public class CsvWriter : DynamicSink
    {
        static readonly MethodInfo writeLineMethod = typeof(StreamWriter).GetMethods().First(m => m.Name == "WriteLine" && m.GetParameters().Length == 0);
        static readonly MethodInfo writeMethod = typeof(StreamWriter).GetMethods().First(m => m.Name == "Write" &&
                                                                                              m.GetParameters().Length == 2 &&
                                                                                              m.GetParameters()[1].ParameterType == typeof(object));

        [Description("The name of the output file.")]
        [FileNameFilter("CSV (Comma delimited)|*.csv|All Files|*.*")]
        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string FileName { get; set; }

        [Description("Indicates whether to append or overwrite the specified file.")]
        public bool Append { get; set; }

        [Description("The optional suffix used to create file names.")]
        public PathSuffix Suffix { get; set; }

        [Description("Indicates whether to include a text header with column names for multi-dimensional input.")]
        public bool IncludeHeader { get; set; }

        protected override Sink<T> CreateProcessor<T>()
        {
            return new CsvProcessor<T>(this);
        }

        class CsvProcessor<T> : Sink<T>
        {
            Task writerTask;
            StreamWriter writer;
            Action<T> writeAction;
            CsvWriter parent;

            public CsvProcessor(CsvWriter owner)
            {
                this.parent = owner;
            }

            public override void Process(T input)
            {
                writerTask = writerTask.ContinueWith(task => writeAction(input));
            }

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

            static IEnumerable<Expression> MakeMemberAccess(Expression expression)
            {
                var stack = new Stack<ExpressionNode>();
                stack.Push(new ExpressionNode { Expression = expression });
                while (stack.Count > 0)
                {
                    var current = stack.Pop();
                    expression = current.Expression;
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
                return Enumerable.Concat<MemberInfo>(
                    type.GetFields(BindingFlags.Instance | BindingFlags.Public),
                    type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                    .Where(member => member.GetCustomAttributes(typeof(XmlIgnoreAttribute), true).Length == 0);
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

            public override IDisposable Load()
            {
                const string ParameterName = "input";
                const string EntryFormat = "{0} ";

                if (!string.IsNullOrEmpty(parent.FileName))
                {
                    var fileName = PathHelper.AppendSuffix(parent.FileName, parent.Suffix);
                    writer = new StreamWriter(fileName, parent.Append, Encoding.ASCII);

                    var parameterType = typeof(T);
                    var parameter = Expression.Parameter(parameterType, ParameterName);

                    var formatConstant = Expression.Constant(EntryFormat);
                    var writerConstant = Expression.Constant(writer);

                    var memberAccessExpressions = MakeMemberAccess(parameter).ToArray();
                    Array.Reverse(memberAccessExpressions);

                    var writeParameters = from memberAccess in memberAccessExpressions
                                          let memberString = GetDataString(memberAccess)
                                          select Expression.Call(writerConstant, writeMethod, formatConstant, memberString);
                    var writeLineExpression = Expression.Call(writerConstant, writeLineMethod);
                    var bodyExpressions = writeParameters.Concat(Enumerable.Repeat(writeLineExpression, 1)).ToArray();

                    var body = Expression.Block(bodyExpressions);
                    writeAction = Expression.Lambda<Action<T>>(body, parameter).Compile();

                    var header = string.Empty;
                    if (parent.IncludeHeader)
                    {
                        foreach (var memberAccess in memberAccessExpressions)
                        {
                            var accessExpression = memberAccess.ToString();
                            header += string.Format(EntryFormat, accessExpression.Remove(accessExpression.IndexOf(ParameterName), ParameterName.Length)).TrimStart('.');
                        }
                        header = header.Trim();
                    }

                    writerTask = new Task(() => { if (!string.IsNullOrEmpty(header)) writer.WriteLine(header); });
                    writerTask.Start();
                }

                return base.Load();
            }

            protected override void Unload()
            {
                var closingWriter = writer;
                writerTask.ContinueWith(task => closingWriter.Close());
                writerTask = null;
                writer = null;
                base.Unload();
            }
        }
    }
}

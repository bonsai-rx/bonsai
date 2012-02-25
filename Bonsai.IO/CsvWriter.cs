using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.ComponentModel;

namespace Bonsai.IO
{
    public class CsvWriter : DynamicSink
    {
        static readonly MethodInfo writeLineMethod = typeof(StreamWriter).GetMethods().First(m => m.Name == "WriteLine" && m.GetParameters().Length == 0);
        static readonly MethodInfo writeMethod = typeof(StreamWriter).GetMethods().First(m => m.Name == "Write" &&
                                                                                              m.GetParameters().Length == 2 &&
                                                                                              m.GetParameters()[1].ParameterType == typeof(object));

        [FileNameFilter("CSV (Comma delimited)|*.csv|All Files|*.*")]
        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string FileName { get; set; }

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

            public CsvProcessor(CsvWriter parent)
            {
                this.parent = parent;
            }

            public override void Process(T input)
            {
                writerTask = writerTask.ContinueWith(task => writeAction(input));
            }

            IEnumerable<MemberInfo> GetDataMembers(Type type)
            {
                return Enumerable.Concat<MemberInfo>(
                    type.GetFields(BindingFlags.Instance | BindingFlags.Public),
                    type.GetProperties(BindingFlags.Instance | BindingFlags.Public));
            }

            IEnumerable<Expression> MakeMemberAccess(Expression expression)
            {
                if (expression.Type.IsPrimitive || expression.Type.IsEnum || expression.Type == typeof(string))
                {
                    return Enumerable.Repeat(expression, 1);
                }

                return from member in GetDataMembers(expression.Type)
                       let memberAccess = Expression.MakeMemberAccess(expression, member)
                       where memberAccess.Type.IsValueType
                       from memberExpression in MakeMemberAccess(memberAccess)
                       select memberExpression;
            }

            public override IDisposable Load()
            {
                const string ParameterName = "Value";
                const string EntryFormat = "{0} ";

                if (!string.IsNullOrEmpty(parent.FileName))
                {
                    writer = new StreamWriter(parent.FileName, false, Encoding.ASCII);

                    var parameterType = typeof(T);
                    var parameter = Expression.Parameter(parameterType, ParameterName);

                    var formatConstant = Expression.Constant(EntryFormat);
                    var writerConstant = Expression.Constant(writer);

                    var memberAccessExpressions = MakeMemberAccess(parameter).ToArray();
                    var writeParameters = from memberAccess in memberAccessExpressions
                                          let memberString = Expression.Call(memberAccess, "ToString", null)
                                          select Expression.Call(writerConstant, writeMethod, formatConstant, memberString);
                    var writeLineExpression = Expression.Call(writerConstant, writeLineMethod);
                    var bodyExpressions = writeParameters.Concat(Enumerable.Repeat(writeLineExpression, 1)).ToArray();

                    var body = Expression.Block(bodyExpressions);
                    writeAction = Expression.Lambda<Action<T>>(body, parameter).Compile();

                    var header = string.Empty;
                    foreach (var memberAccess in memberAccessExpressions)
                    {
                        var accessExpression = memberAccess.ToString();
                        header += string.Format(EntryFormat, accessExpression.Remove(accessExpression.IndexOf(ParameterName), ParameterName.Length)).TrimStart('.');
                    }
                    header = header.Trim();

                    writerTask = new Task(() => { if (!string.IsNullOrEmpty(header)) writer.WriteLine(header); });
                    writerTask.Start();
                }

                return base.Load();
            }

            protected override void Unload()
            {
                writerTask.Wait();
                writer.Close();
                base.Unload();
            }
        }
    }
}

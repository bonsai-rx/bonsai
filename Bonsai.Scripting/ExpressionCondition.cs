using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Scripting
{
    [DefaultProperty("Expression")]
    [WorkflowElementCategory(ElementCategory.Condition)]
    [TypeDescriptionProvider(typeof(ExpressionConditionTypeDescriptionProvider))]
    public class ExpressionCondition : SingleArgumentExpressionBuilder, IScriptingElement
    {
        static readonly MethodInfo whereMethod = typeof(Observable).GetMethods()
                                                                   .Single(m => m.Name == "Where" &&
                                                                           m.GetParameters().Length == 2 &&
                                                                           m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>));

        public ExpressionCondition()
        {
            Expression = "it";
        }

        [Category("Design")]
        [Externalizable(false)]
        [Description("The name of the expression condition.")]
        public string Name { get; set; }

        [Category("Design")]
        [Externalizable(false)]
        [Description("A description for the expression condition.")]
        [Editor(DesignTypes.MultilineStringEditor, typeof(UITypeEditor))]
        public string Description { get; set; }

        [Editor(typeof(ExpressionScriptEditor), typeof(UITypeEditor))]
        [Description("The expression that determines which elements to filter.")]
        public string Expression { get; set; }

        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var source = arguments.First();
            var sourceType = source.Type.GetGenericArguments()[0];
            var predicate = global::System.Linq.Dynamic.DynamicExpression.ParseLambda(sourceType, typeof(bool), Expression);
            return System.Linq.Expressions.Expression.Call(whereMethod.MakeGenericMethod(sourceType), source, predicate);
        }

        class ExpressionConditionTypeDescriptionProvider : TypeDescriptionProvider
        {
            static readonly TypeDescriptionProvider parentProvider = TypeDescriptor.GetProvider(typeof(ExpressionCondition));

            public ExpressionConditionTypeDescriptionProvider()
                : base(parentProvider)
            {
            }

            public override ICustomTypeDescriptor GetExtendedTypeDescriptor(object instance)
            {
                return new ScriptingElementTypeDescriptor(instance,
                    "An expression that is used to filter individual elements of the input sequence.");
            }
        }
    }
}

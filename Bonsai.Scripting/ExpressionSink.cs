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
    [WorkflowElementCategory(ElementCategory.Sink)]
    [TypeDescriptionProvider(typeof(ExpressionSinkTypeDescriptionProvider))]
    public class ExpressionSink : SingleArgumentExpressionBuilder, IScriptingElement
    {
        static readonly MethodInfo doMethod = typeof(Observable).GetMethods()
                                                                .Single(m => m.Name == "Do" &&
                                                                        m.GetParameters().Length == 2 &&
                                                                        m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Action<>));

        [Category("Design")]
        [Externalizable(false)]
        [Description("The name of the expression sink.")]
        public string Name { get; set; }

        [Category("Design")]
        [Externalizable(false)]
        [Description("A description for the expression sink.")]
        [Editor(DesignTypes.MultilineStringEditor, typeof(UITypeEditor))]
        public string Description { get; set; }
        
        [Editor(typeof(ExpressionScriptEditor), typeof(UITypeEditor))]
        [Description("The expression that determines the action to perform on the input elements.")]
        public string Expression { get; set; }

        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var expression = Expression;
            var source = arguments.First();
            if (!string.IsNullOrEmpty(expression))
            {
                var sourceType = source.Type.GetGenericArguments()[0];
                var actionType = System.Linq.Expressions.Expression.GetActionType(sourceType);
                var itParameter = new[] { System.Linq.Expressions.Expression.Parameter(sourceType, string.Empty) };
                var onNext = global::System.Linq.Dynamic.DynamicExpression.ParseLambda(actionType, itParameter, null, Expression);
                return System.Linq.Expressions.Expression.Call(doMethod.MakeGenericMethod(sourceType), source, onNext);
            }
            else return source;
        }

        class ExpressionSinkTypeDescriptionProvider : TypeDescriptionProvider
        {
            static readonly TypeDescriptionProvider parentProvider = TypeDescriptor.GetProvider(typeof(ExpressionSink));

            public ExpressionSinkTypeDescriptionProvider()
                : base(parentProvider)
            {
            }

            public override ICustomTypeDescriptor GetExtendedTypeDescriptor(object instance)
            {
                return new ScriptingElementTypeDescriptor(instance,
                    "An expression that is used to perform an action on individual elements of the input sequence.");
            }
        }
    }
}

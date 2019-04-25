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
    [WorkflowElementCategory(ElementCategory.Transform)]
    [TypeDescriptionProvider(typeof(ExpressionTransformTypeDescriptionProvider))]
    public class ExpressionTransform : SingleArgumentExpressionBuilder, IScriptingElement
    {
        static readonly MethodInfo selectMethod = typeof(Observable).GetMethods()
                                                                    .Single(m => m.Name == "Select" &&
                                                                            m.GetParameters().Length == 2 &&
                                                                            m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>));

        public ExpressionTransform()
        {
            Expression = "it";
        }

        [Category("Design")]
        [Externalizable(false)]
        [Description("The name of the expression transform.")]
        public string Name { get; set; }

        [Category("Design")]
        [Externalizable(false)]
        [Description("A description for the expression transform.")]
        [Editor(DesignTypes.MultilineStringEditor, typeof(UITypeEditor))]
        public string Description { get; set; }

        [Editor(typeof(ExpressionScriptEditor), typeof(UITypeEditor))]
        [Description("The expression that determines the operation of the transform.")]
        public string Expression { get; set; }

        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var source = arguments.First();
            var sourceType = source.Type.GetGenericArguments()[0];
            var selector = global::System.Linq.Dynamic.DynamicExpression.ParseLambda(sourceType, null, Expression);
            return System.Linq.Expressions.Expression.Call(selectMethod.MakeGenericMethod(sourceType, selector.ReturnType), source, selector);
        }

        class ExpressionTransformTypeDescriptionProvider : TypeDescriptionProvider
        {
            static readonly TypeDescriptionProvider parentProvider = TypeDescriptor.GetProvider(typeof(ExpressionTransform));

            public ExpressionTransformTypeDescriptionProvider()
                : base(parentProvider)
            {
            }

            public override ICustomTypeDescriptor GetExtendedTypeDescriptor(object instance)
            {
                return new ScriptingElementTypeDescriptor(instance,
                    "An expression that is used to process and convert individual elements of the input sequence.");
            }
        }
    }
}

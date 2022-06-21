using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;

namespace Bonsai.Scripting.Expressions
{
    /// <summary>
    /// Represents an operator that uses an expression script to transform each
    /// element of an observable sequence.
    /// </summary>
    [DefaultProperty(nameof(Expression))]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [TypeDescriptionProvider(typeof(ExpressionTransformTypeDescriptionProvider))]
    [Description("An expression script used to transform each element of the sequence.")]
    public class ExpressionTransform : SingleArgumentExpressionBuilder, IScriptingElement
    {
        static readonly MethodInfo selectMethod = typeof(Observable).GetMethods()
                                                                    .Single(m => m.Name == nameof(Observable.Select) &&
                                                                            m.GetParameters().Length == 2 &&
                                                                            m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>));

        /// <summary>
        /// Gets or sets the name of the expression transform.
        /// </summary>
        [Externalizable(false)]
        [Category(nameof(CategoryAttribute.Design))]
        [Description("The name of the expression transform.")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a description for the expression transform.
        /// </summary>
        [Externalizable(false)]
        [Category(nameof(CategoryAttribute.Design))]
        [Description("A description for the expression transform.")]
        [Editor(DesignTypes.MultilineStringEditor, DesignTypes.UITypeEditor)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the expression that determines the operation of the transform.
        /// </summary>
        [Editor("Bonsai.Scripting.Expressions.Design.ExpressionScriptEditor, Bonsai.Scripting.Expressions.Design", DesignTypes.UITypeEditor)]
        [Description("The expression that determines the operation of the transform.")]
        public string Expression { get; set; } = "it";

        /// <inheritdoc/>
        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var source = arguments.First();
            var sourceType = source.Type.GetGenericArguments()[0];
            var selector = System.Linq.Dynamic.DynamicExpression.ParseLambda(sourceType, null, Expression);
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
                    "An expression script used to transform each element of the sequence.");
            }
        }
    }
}

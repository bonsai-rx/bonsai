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
    /// Represents an operator that uses an expression script to filter the elements
    /// of an observable sequence.
    /// </summary>
    [DefaultProperty(nameof(Expression))]
    [TypeDescriptionProvider(typeof(ExpressionConditionTypeDescriptionProvider))]
    [Description("An expression script used to filter the elements of the sequence.")]
    public class ExpressionCondition : SingleArgumentExpressionBuilder, IScriptingElement
    {
        static readonly MethodInfo whereMethod = typeof(Observable).GetMethods()
                                                                   .Single(m => m.Name == nameof(Observable.Where) &&
                                                                           m.GetParameters().Length == 2 &&
                                                                           m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>));

        /// <summary>
        /// Gets or sets the name of the expression condition.
        /// </summary>
        [Externalizable(false)]
        [Category(nameof(CategoryAttribute.Design))]
        [Description("The name of the expression condition.")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a description for the expression condition.
        /// </summary>
        [Externalizable(false)]
        [Category(nameof(CategoryAttribute.Design))]
        [Description("A description for the expression condition.")]
        [Editor(DesignTypes.MultilineStringEditor, DesignTypes.UITypeEditor)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the expression that determines which elements to filter.
        /// </summary>
        [Editor("Bonsai.Scripting.Expressions.Design.ExpressionScriptEditor, Bonsai.Scripting.Expressions.Design", DesignTypes.UITypeEditor)]
        [Description("The expression that determines which elements to filter.")]
        public string Expression { get; set; } = "it";

        /// <inheritdoc/>
        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var source = arguments.First();
            var sourceType = source.Type.GetGenericArguments()[0];
            var predicate = System.Linq.Dynamic.DynamicExpression.ParseLambda(sourceType, typeof(bool), Expression);
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
                    "An expression script used to filter the elements of the sequence.");
            }
        }
    }
}

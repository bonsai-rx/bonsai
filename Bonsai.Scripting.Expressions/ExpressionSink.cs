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
    /// Represents an operator that uses an expression script to invoke an action for
    /// each element of an observable sequence.
    /// </summary>
    [DefaultProperty(nameof(Expression))]
    [WorkflowElementCategory(ElementCategory.Sink)]
    [TypeDescriptionProvider(typeof(ExpressionSinkTypeDescriptionProvider))]
    [Description("An expression script used to invoke an action for each element of the sequence.")]
    public class ExpressionSink : SingleArgumentExpressionBuilder, IScriptingElement
    {
        static readonly MethodInfo doMethod = typeof(Observable).GetMethods()
                                                                .Single(m => m.Name == nameof(Observable.Do) &&
                                                                        m.GetParameters().Length == 2 &&
                                                                        m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Action<>));

        /// <summary>
        /// Gets or sets the name of the expression sink.
        /// </summary>
        [Externalizable(false)]
        [Category(nameof(CategoryAttribute.Design))]
        [Description("The name of the expression sink.")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a description for the expression sink.
        /// </summary>
        [Externalizable(false)]
        [Category(nameof(CategoryAttribute.Design))]
        [Description("A description for the expression sink.")]
        [Editor(DesignTypes.MultilineStringEditor, DesignTypes.UITypeEditor)]
        public string Description { get; set; }
        
        /// <summary>
        /// Gets or sets the expression that determines the action to perform
        /// on the input elements.
        /// </summary>
        [Editor("Bonsai.Scripting.Expressions.Design.ExpressionScriptEditor, Bonsai.Scripting.Expressions.Design", DesignTypes.UITypeEditor)]
        [Description("The expression that determines the action to perform on the input elements.")]
        public string Expression { get; set; }

        /// <inheritdoc/>
        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var expression = Expression;
            var source = arguments.First();
            if (!string.IsNullOrEmpty(expression))
            {
                var sourceType = source.Type.GetGenericArguments()[0];
                var actionType = System.Linq.Expressions.Expression.GetActionType(sourceType);
                var itParameter = new[] { System.Linq.Expressions.Expression.Parameter(sourceType, string.Empty) };
                var onNext = System.Linq.Dynamic.DynamicExpression.ParseLambda(actionType, itParameter, null, Expression);
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
                    "An expression script used to invoke an action for each element of the sequence.");
            }
        }
    }
}

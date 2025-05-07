using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reflection;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that calls the specified instance method using the
    /// arguments selected from the elements of an observable sequence.
    /// </summary>
    [DefaultProperty(nameof(MethodName))]
    [XmlType("Call", Namespace = Constants.XmlNamespace)]
    [Description("Calls the specified instance method using the arguments selected from the elements of an observable sequence.")]
    public class CallBuilder : SelectBuilder
    {
        /// <summary>
        /// Gets or sets the name of the method to call.
        /// </summary>
        [TypeConverter(typeof(MethodNameConverter))]
        [Description("The name of the method to call.")]
        public string MethodName { get; set; } = nameof(ToString);

        /// <summary>
        /// Gets or sets the name of the property that will be used as the instance for
        /// the method call.
        /// </summary>
        [Editor("Bonsai.Design.MemberSelectorEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [Description("The name of the property that will be used as the instance for the method call.")]
        public string InstanceSelector { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the inner properties used as arguments
        /// in each method call.
        /// </summary>
        [Description("Specifies the inner properties used as arguments in each method call.")]
        [Editor("Bonsai.Design.MultiMemberSelectorEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string ArgumentSelector { get; set; }

        /// <summary>
        /// Returns the expression that calls the specified instance method with the
        /// selected arguments.
        /// </summary>
        /// <param name="expression">
        /// The expression representing each element of an observable sequence.
        /// </param>
        /// <returns>
        /// The <see cref="Expression"/> that calls the instance method overload that
        /// best matches the selected arguments.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The method name is null or empty.
        /// </exception>
        protected override Expression BuildSelector(Expression expression)
        {
            var methodName = MethodName;
            if (string.IsNullOrEmpty(methodName))
                throw new InvalidOperationException("The method name must not be null or empty.");

            var argumentSelector = ArgumentSelector;
            var arguments = !string.IsNullOrEmpty(argumentSelector)
                ? ExpressionHelper.SelectMembers(expression, ArgumentSelector).ToArray()
                : Array.Empty<Expression>();

            var instance = ExpressionHelper.MemberAccess(expression, InstanceSelector);
            var methods = GetInstanceMethods(instance.Type).Where(m => m.Name == methodName);
            return BuildCall(instance, methods, arguments);
        }

        static IEnumerable<MethodInfo> GetInstanceMethods(Type type)
        {
            const BindingFlags bindingAttributes = BindingFlags.Instance | BindingFlags.Public;
            return type.GetMethods(bindingAttributes).Where(m => !m.IsSpecialName);
        }

        class MethodNameConverter : StringConverter
        {
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                if (context is not null && context.Instance is CallBuilder callBuilder)
                {
                    var workflow = (ExpressionBuilderGraph)context.GetService(typeof(ExpressionBuilderGraph));
                    var source = (from node in workflow
                                  from successor in node.Successors
                                  where Unwrap(successor.Target.Value) == callBuilder
                                  select node.Value).FirstOrDefault();
                    if (source is not null)
                    {
                        var workflowBuilder = (WorkflowBuilder)context.GetService(typeof(WorkflowBuilder));
                        var sourceExpression = workflowBuilder.Workflow.Build(source);
                        var instanceType = sourceExpression.Type.GetGenericArguments()[0];
                        var instanceExpression = ExpressionHelper.MemberAccess(
                            Expression.Parameter(instanceType),
                            callBuilder.InstanceSelector);
                        var methodNames = GetInstanceMethods(instanceExpression.Type)
                            .Select(method => method.Name)
                            .Distinct()
                            .ToList();
                        return new StandardValuesCollection(methodNames);
                    }
                }

                return base.GetStandardValues(context);
            }
        }
    }
}

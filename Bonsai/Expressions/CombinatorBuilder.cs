using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Expressions
{
    [XmlType("Combinator", Namespace = Constants.XmlNamespace)]
    public class CombinatorBuilder : CombinatorExpressionBuilder, INamedElement
    {
        object combinator;

        public CombinatorBuilder()
            : base(minArguments: 0, maxArguments: 0)
        {
        }

        public string Name
        {
            get { return GetElementDisplayName(combinator); }
        }

        public object Combinator
        {
            get { return combinator; }
            set
            {
                combinator = value;
                ArgumentRange = GetArgumentRange();
            }
        }

        Range<int> GetArgumentRange()
        {
            if (combinator == null) return Range.Create(0, 0);
            var combinatorType = combinator.GetType();
            var processMethodParameters = GetProcessMethods(combinatorType).Select(m => m.GetParameters()).ToArray();
            var paramArray = processMethodParameters.Any(p =>
                p.Length == 1 &&
                Attribute.IsDefined(p[0], typeof(ParamArrayAttribute)));

            var min = processMethodParameters.Min(p => p.Length);
            var max = paramArray ? int.MaxValue : processMethodParameters.Max(p => p.Length);
            return Range.Create(min, max);
        }

        static IEnumerable<MethodInfo> GetProcessMethods(Type combinatorType)
        {
            const BindingFlags bindingAttributes = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var combinatorAttributes = combinatorType.GetCustomAttributes(typeof(CombinatorAttribute), true);
            var methodName = ((CombinatorAttribute)combinatorAttributes.Single()).MethodName;
            return combinatorType.GetMethods(bindingAttributes).Where(m => m.Name == methodName);
        }

        public override Expression Build()
        {
            var output = BuildCombinator();
            var combinatorExpression = Expression.Constant(Combinator);
            return BuildMappingOutput(combinatorExpression, output, PropertyMappings);
        }

        protected override Expression BuildCombinator()
        {
            var combinatorExpression = Expression.Constant(Combinator);
            var processMethods = GetProcessMethods(combinatorExpression.Type);
            return BuildCall(combinatorExpression, processMethods, Arguments.Values.Take(ArgumentRange.UpperBound).ToArray());
        }
    }
}

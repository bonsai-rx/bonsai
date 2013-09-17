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
    [SourceMapping]
    [PropertyMapping]
    [XmlType("Combinator", Namespace = Constants.XmlNamespace)]
    public class CombinatorBuilder : ExpressionBuilder
    {
        object combinator;
        Range<int> argumentRange;
        readonly PropertyMappingCollection propertyMappings;

        public CombinatorBuilder()
        {
            argumentRange = Range.Create(0, 0);
            propertyMappings = new PropertyMappingCollection();
        }

        public override Range<int> ArgumentRange
        {
            get { return argumentRange; }
        }

        public object Combinator
        {
            get { return combinator; }
            set
            {
                combinator = value;
                argumentRange = GetArgumentRange();
            }
        }

        public string MemberSelector { get; set; }

        public PropertyMappingCollection PropertyMappings
        {
            get { return propertyMappings; }
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
            var combinatorExpression = Expression.Constant(Combinator);
            var processMethods = GetProcessMethods(combinatorExpression.Type);
            if (Arguments.Count == 1)
            {
                return BuildCallRemapping(
                    combinatorExpression,
                    (combinator, sourceSelect) => HandleBuildException(BuildCall(combinator, processMethods, sourceSelect), this),
                    Arguments.Values.Single(),
                    MemberSelector,
                    propertyMappings);
            }
            else return BuildCall(combinatorExpression, processMethods, Arguments.Values.ToArray());
        }
    }
}

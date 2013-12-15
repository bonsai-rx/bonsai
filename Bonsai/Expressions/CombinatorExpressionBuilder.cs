using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    [PropertyMapping]
    public abstract class CombinatorExpressionBuilder : VariableArgumentExpressionBuilder
    {
        readonly PropertyMappingCollection propertyMappings = new PropertyMappingCollection();

        protected CombinatorExpressionBuilder(int minArguments, int maxArguments)
            : base(minArguments, maxArguments)
        {
        }

        [Browsable(false)]
        public PropertyMappingCollection PropertyMappings
        {
            get { return propertyMappings; }
        }

        public override Expression Build()
        {
            var output = BuildCombinator();
            var combinatorExpression = Expression.Constant(this);
            return BuildMappingOutput(combinatorExpression, output, propertyMappings);
        }

        protected abstract Expression BuildCombinator();
    }
}

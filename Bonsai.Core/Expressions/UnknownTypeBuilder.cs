using Bonsai.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that is a proxy for an unknown type.
    /// </summary>
    public abstract class UnknownTypeBuilder : ExpressionBuilder
    {
        /// <summary>
        /// Gets the range of input arguments that this expression builder accepts.
        /// </summary>
        public override Range<int> ArgumentRange
        {
            get { return Range.Create(0, int.MaxValue); }
        }

        /// <summary>
        /// Throws a <see cref="System.NotImplementedException"/> by design in order to indicate
        /// the current builder is a proxy for an unknown type.
        /// </summary>
        /// <param name="arguments">
        /// A collection of <see cref="Expression"/> nodes that represents the input arguments.
        /// </param>
        /// <returns>This method never returns.</returns>
        /// <exception cref="System.NotImplementedException">
        /// This method always throws this exception, by design.
        /// </exception>
        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var type = GetType();
            var descriptionAttribute = type.GetCustomAttribute<DescriptionAttribute>();
            if (descriptionAttribute == null)
            {
                var description = string.Format(Resources.Exception_UnknownTypeBuilder, type.FullName);
                descriptionAttribute = new DescriptionAttribute(description);
            }

            throw new NotImplementedException(descriptionAttribute.Description);
        }
    }
}

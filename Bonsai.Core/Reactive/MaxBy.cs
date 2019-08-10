using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents a combinator that returns the elements in the observable
    /// sequence with the maximum key value.
    /// </summary>
    [DefaultProperty("KeySelector")]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Returns the elements in the observable sequence with the maximum key value.")]
    public class MaxBy : SingleArgumentExpressionBuilder
    {
        static readonly MethodInfo maxBy = typeof(Observable).GetMethods()
                                                             .Single(m => m.Name == "MaxBy" &&
                                                                          m.GetParameters().Length == 2);

        /// <summary>
        /// Gets or sets a string used to specify a key for each element of the observable sequence.
        /// </summary>
        [Description("The inner properties that will be used as key for each element of the sequence.")]
        [Editor("Bonsai.Design.MultiMemberSelectorEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string KeySelector { get; set; }

        /// <summary>
        /// Generates an <see cref="Expression"/> node from a collection of input arguments.
        /// The result can be chained with other builders in a workflow.
        /// </summary>
        /// <param name="arguments">
        /// A collection of <see cref="Expression"/> nodes that represents the input arguments.
        /// </param>
        /// <returns>An <see cref="Expression"/> tree node.</returns>
        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var source = arguments.First();
            var parameterType = source.Type.GetGenericArguments()[0];
            var parameter = Expression.Parameter(parameterType);
            var keySelectorBody = MemberSelector(parameter, KeySelector);
            var keySelector = Expression.Lambda(keySelectorBody, parameter);
            return Expression.Call(maxBy.MakeGenericMethod(parameterType, keySelector.ReturnType), source, keySelector);
        }
    }

    /// <summary>
    /// This type is obsolete. Please use the <see cref="MaxBy"/> operator instead.
    /// </summary>
    [Obsolete]
    [ProxyType(typeof(MaxBy))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class MaxByBuilder : MaxBy
    {
    }
}

using System.ComponentModel;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Provides a base class for expression builders that declare shared subjects. This is an abstract class.
    /// </summary>
    [DefaultProperty(nameof(Name))]
    [WorkflowElementIcon(nameof(SubjectExpressionBuilder))]
    public abstract class SubjectExpressionBuilder : VariableArgumentExpressionBuilder, INamedElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubjectExpressionBuilder"/> class
        /// with the specified argument range.
        /// </summary>
        /// <param name="minArguments">The inclusive lower bound of the argument range.</param>
        /// <param name="maxArguments">The inclusive upper bound of the argument range.</param>
        protected SubjectExpressionBuilder(int minArguments, int maxArguments)
            : base(minArguments, maxArguments)
        {
        }

        /// <summary>
        /// Gets or sets the name of the shared subject.
        /// </summary>
        [Category("Subject")]
        [Description("The name of the shared subject.")]
        public string Name { get; set; }
    }
}

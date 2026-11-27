namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder workflow container.
    /// </summary>
    public interface IWorkflowBuilder
    {
        /// <summary>
        /// Gets the <see cref="ExpressionBuilderGraph"/> instance used by this builder.
        /// </summary>
        ExpressionBuilderGraph Workflow { get; }
    }
}

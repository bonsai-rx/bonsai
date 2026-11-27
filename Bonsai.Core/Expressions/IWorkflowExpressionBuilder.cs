namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents expression builder instances that generate their output by means
    /// of an encapsulated workflow.
    /// </summary>
    public interface IWorkflowExpressionBuilder : IExpressionBuilder, IWorkflowBuilder
    {
        /// <summary>
        /// Gets the expression builder workflow that will be used to generate the
        /// output expression tree.
        /// </summary>
        new ExpressionBuilderGraph Workflow { get; }
    }
}

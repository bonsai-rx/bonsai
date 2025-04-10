using System;
using Bonsai.Expressions;

namespace Bonsai.Editor.GraphModel
{
    internal class WorkflowQueryResult
    {
        public WorkflowQueryResult(ExpressionBuilder builder, WorkflowEditorPath path)
        {
            Builder = builder ?? throw new ArgumentNullException(nameof(builder));
            Path = path ?? throw new ArgumentNullException(nameof(path));
        }

        public ExpressionBuilder Builder { get; }

        public WorkflowEditorPath Path { get; }
    }
}

using System.ComponentModel;

namespace Bonsai.Expressions
{
    class WorkflowTypeDescriptionProvider : TypeDescriptionProvider
    {
        static readonly TypeDescriptionProvider parentProvider = TypeDescriptor.GetProvider(typeof(WorkflowExpressionBuilder));

        public WorkflowTypeDescriptionProvider()
            : base(parentProvider)
        {
        }

        public override ICustomTypeDescriptor GetExtendedTypeDescriptor(object instance)
        {
            return new WorkflowTypeDescriptor(instance);
        }
    }
}

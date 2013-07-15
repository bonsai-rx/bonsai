using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class WorkflowElementCategoryAttribute : Attribute
    {
        public static readonly WorkflowElementCategoryAttribute Default = new WorkflowElementCategoryAttribute(ElementCategory.Combinator);

        public WorkflowElementCategoryAttribute(ElementCategory category)
        {
            Category = category;
        }

        public ElementCategory Category { get; private set; }
    }
}

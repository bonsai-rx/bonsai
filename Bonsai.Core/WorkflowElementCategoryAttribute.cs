using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    /// <summary>
    /// Determines the category of the workflow element this attribute is bound to.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class WorkflowElementCategoryAttribute : Attribute
    {
        /// <summary>
        /// Specifies the default value for the <see cref="WorkflowElementCategoryAttribute"/>. This field is read-only.
        /// </summary>
        public static readonly WorkflowElementCategoryAttribute Default = new WorkflowElementCategoryAttribute(ElementCategory.Combinator);

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowElementCategoryAttribute"/> class with
        /// the specified category.
        /// </summary>
        /// <param name="category">The category of the workflow element this attribute is bound to.</param>
        public WorkflowElementCategoryAttribute(ElementCategory category)
        {
            Category = category;
        }

        /// <summary>
        /// Gets the category of the workflow element this attribute is bound to.
        /// </summary>
        public ElementCategory Category { get; private set; }
    }
}

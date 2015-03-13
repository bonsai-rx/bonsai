using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Expressions;

namespace Bonsai
{
    /// <summary>
    /// Specifies the category of a workflow element.
    /// </summary>
    public enum ElementCategory
    {
        /// <summary>
        /// A generator of observable sequences of data elements.
        /// </summary>
        Source,

        /// <summary>
        /// A combinator that propagates only the elements of an observable sequence
        /// matching some specific condition.
        /// </summary>
        Condition,

        /// <summary>
        /// A combinator that transforms every element of an observable sequence into
        /// a new data element.
        /// </summary>
        Transform,

        /// <summary>
        /// A combinator that introduces side effects on an observable sequence without
        /// modifying its elements.
        /// </summary>
        Sink,

        /// <summary>
        /// A combinator that allows the user to specify its operation in terms of a nested
        /// workflow.
        /// </summary>
        Nested,

        /// <summary>
        /// A generator of observable elements that can be used as a named workflow property.
        /// </summary>
        Property,

        /// <summary>
        /// An operator that can be applied to one or more observable sequences to produce a new
        /// observable sequence.
        /// </summary>
        Combinator,

        /// <summary>
        /// A set of operators defining a data processing workflow.
        /// </summary>
        Workflow
    }
}

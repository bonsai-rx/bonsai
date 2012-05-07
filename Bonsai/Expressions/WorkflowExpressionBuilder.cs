using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Xml.Serialization;
using System.ComponentModel;
using Bonsai.Dag;

namespace Bonsai.Expressions
{
    [XmlType("Workflow", Namespace = Constants.XmlNamespace)]
    public abstract class WorkflowExpressionBuilder : CombinatorExpressionBuilder
    {
        readonly ExpressionBuilderGraph workflow;

        protected WorkflowExpressionBuilder()
            : this(new ExpressionBuilderGraph())
        {
        }

        protected WorkflowExpressionBuilder(ExpressionBuilderGraph workflow)
        {
            if (workflow == null)
            {
                throw new ArgumentNullException("workflow");
            }

            this.workflow = workflow;
        }

        public string Name { get; set; }

        [XmlIgnore]
        [Browsable(false)]
        public ExpressionBuilderGraph Workflow
        {
            get { return workflow; }
        }

        [Browsable(false)]
        [XmlElement("Workflow")]
        public ExpressionBuilderGraphDescriptor WorkflowDescriptor
        {
            get { return workflow.ToDescriptor(); }
            set
            {
                workflow.Clear();
                workflow.AddDescriptor(value);
            }
        }
    }
}

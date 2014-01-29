﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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

        class WorkflowTypeDescriptor : CustomTypeDescriptor
        {
            WorkflowExpressionBuilder builder;
            static readonly Attribute[] emptyAttributes = new Attribute[0];

            public WorkflowTypeDescriptor(object instance)
            {
                if (instance == null)
                {
                    throw new ArgumentNullException("instance");
                }

                builder = (WorkflowExpressionBuilder)instance;
            }

            public override PropertyDescriptorCollection GetProperties()
            {
                return GetProperties(emptyAttributes);
            }

            public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
            {
                var properties = from node in builder.Workflow
                                 let property = ExpressionBuilder.GetWorkflowElement(node.Value) as WorkflowProperty
                                 where property != null
                                 let name = property.Name
                                 where !string.IsNullOrEmpty(name)
                                 select new WorkflowPropertyDescriptor(name, emptyAttributes, property);
                return new PropertyDescriptorCollection(properties.ToArray());
            }
        }
    }
}

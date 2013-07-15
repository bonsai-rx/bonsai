﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    [WorkflowElementCategory(ElementCategory.Source)]
    [XmlType("Source", Namespace = Constants.XmlNamespace)]
    public class SourceBuilder : ExpressionBuilder
    {
        public LoadableElement Source { get; set; }

        public override Expression Build()
        {
            var source = Expression.Constant(Source);
            return Expression.Property(source, "Output");
        }
    }
}

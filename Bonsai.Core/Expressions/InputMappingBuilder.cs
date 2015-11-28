﻿using Bonsai.Dag;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that selects inner properties of elements of the sequence
    /// and assigns their values to properties of a workflow element.
    /// </summary>
    [XmlType("InputMapping", Namespace = Constants.XmlNamespace)]
    [Description("Selects inner properties of elements of the sequence and assigns their values to properties of a workflow element.")]
    public class InputMappingBuilder : PropertyMappingBuilder
    {
        readonly MemberSelectorBuilder selector = new MemberSelectorBuilder();

        /// <summary>
        /// Gets or sets a string used to select the input element member to project
        /// as output of the sequence.
        /// </summary>
        [Description("The inner properties that will be selected for each element of the sequence.")]
        [Editor("Bonsai.Design.MultiMemberSelectorEditor, Bonsai.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string Selector
        {
            get { return selector.Selector; }
            set { selector.Selector = value; }
        }

        internal override bool BuildArgument(Expression source, Edge<ExpressionBuilder, ExpressionBuilderArgument> successor, out Expression argument)
        {
            base.BuildArgument(source, successor, out argument);
            argument = selector.Build(argument);
            return true;
        }
    }
}

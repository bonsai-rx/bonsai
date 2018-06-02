using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents a workflow property containing a single-precision floating-point number.
    /// </summary>
    [DisplayName("Float")]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Represents a workflow property containing a single-precision floating-point number.")]
    public class FloatProperty : WorkflowProperty<float>
    {
        /// <summary>
        /// Gets or sets the value of the property.
        /// </summary>
        [Range(0, 1)]
        [Description("The value of the property.")]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        public new float Value
        {
            get { return base.Value; }
            set { base.Value = value; }
        }
    }
}

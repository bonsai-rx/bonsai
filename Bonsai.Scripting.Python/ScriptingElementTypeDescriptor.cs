using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Scripting
{
    class ScriptingElementTypeDescriptor : CustomTypeDescriptor
    {
        readonly IScriptingElement element;
        readonly string defaultDescription;

        public ScriptingElementTypeDescriptor(object instance, string description)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            element = instance as IScriptingElement;
            defaultDescription = description;
        }

        public override AttributeCollection GetAttributes()
        {
            var description = defaultDescription;
            if (element != null && !string.IsNullOrEmpty(element.Description))
            {
                description = element.Description;
            }

            return new AttributeCollection(new DescriptionAttribute(description));
        }
    }
}

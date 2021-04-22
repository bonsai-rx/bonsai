using System;
using System.ComponentModel;

namespace Bonsai.Scripting.Expressions
{
    class ScriptingElementTypeDescriptor : CustomTypeDescriptor
    {
        readonly IScriptingElement element;
        readonly string defaultDescription;

        public ScriptingElementTypeDescriptor(object instance, string description)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
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

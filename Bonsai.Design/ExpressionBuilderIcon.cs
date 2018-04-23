using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Design
{
    class ExpressionBuilderIcon : WorkflowIcon
    {
        readonly WorkflowIconAttribute attribute;

        public ExpressionBuilderIcon(WorkflowIconAttribute attribute)
            : base(attribute.Name)
        {
            this.attribute = attribute;
            TypeName = attribute.TypeName;
        }

        public string TypeName { get; private set; }

        public override Stream GetStream()
        {
            if (string.IsNullOrEmpty(Name))
            {
                return null;
            }

            var type = Type.GetType(TypeName, false);
            if (type != null)
            {
                return type.Assembly.GetManifestResourceStream(type, Name);
            }
            else if (File.Exists(Name))
            {
                return new FileStream(Name, FileMode.Open, FileAccess.Read);
            }
            else return null;
        }
    }
}

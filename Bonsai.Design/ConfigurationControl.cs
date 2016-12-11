using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Design
{
    [Obsolete]
    public abstract class ConfigurationControlBase : ConfigurationDropDown
    {
        protected override UITypeEditor CreateConfigurationEditor(Type type)
        {
            return CreateCollectionEditor(type);
        }

        protected abstract CollectionEditor CreateCollectionEditor(Type type);
    }

    [Obsolete]
    public abstract class ConfigurationControl : ConfigurationControlBase
    {
        protected override CollectionEditor CreateCollectionEditor(Type type)
        {
            return CreateConfigurationEditor(type);
        }

        protected abstract new CollectionEditor CreateConfigurationEditor(Type type);
    }
}

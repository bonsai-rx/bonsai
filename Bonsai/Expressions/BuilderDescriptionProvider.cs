using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Bonsai.Expressions
{
    public class BuilderDescriptionProvider<T> : TypeDescriptionProvider
    {
        static readonly TypeDescriptionProvider parentProvider = TypeDescriptor.GetProvider(typeof(T));

        public BuilderDescriptionProvider()
            : base(parentProvider)
        {
        }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            ICustomTypeDescriptor parent = base.GetTypeDescriptor(objectType, instance);
            if (instance == null) return parent;

            return new BuilderCustomTypeDescriptor(parent, instance);
        }
    }
}

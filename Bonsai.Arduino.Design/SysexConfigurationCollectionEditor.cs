using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Design;

namespace Bonsai.Arduino.Design
{
    public class SysexConfigurationCollectionEditor : CollectionEditor
    {
        public SysexConfigurationCollectionEditor(Type type)
            : base(type)
        {
        }

        protected override Type[] CreateNewItemTypes()
        {
            return new[]
            {
                typeof(StepperConfiguration)
            };
        }
    }
}

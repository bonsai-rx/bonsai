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

        protected override string GetDisplayText(object value)
        {
            var configuration = value as SysexConfiguration;
            if (configuration != null)
            {
                return configuration.GetType().Name;
            }

            return base.GetDisplayText(value);
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

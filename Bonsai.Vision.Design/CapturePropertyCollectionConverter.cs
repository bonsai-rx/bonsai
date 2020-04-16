using Bonsai.Design;
using System;
using System.ComponentModel;
using System.Linq;

namespace Bonsai.Vision.Design
{
    public class CapturePropertyCollectionConverter : CollectionConverter
    {
        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            var propertyCollection = (CapturePropertyCollection)value;
            if (propertyCollection != null && propertyCollection.Count > 0)
            {
                var properties = (from property in propertyCollection
                                  select new DynamicPropertyDescriptor(
                                      property.Property.ToString(),
                                      typeof(double),
                                      component => property.Value,
                                      (component, propval) =>
                                      {
                                          property.Value = (double)propval;
                                          var capture = propertyCollection.Capture;
                                          if (capture != null)
                                          {
                                              capture.SetProperty(property.Property, property.Value);
                                          }
                                      },
                                      new EditorAttribute(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)))
                                 .ToArray();
                return new PropertyDescriptorCollection(properties);
            }

            return base.GetProperties(context, value, attributes);
        }
    }
}

using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Xml.Serialization;
using OpenCV.Net;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that encodes each image in the sequence into a byte
    /// buffer in memory using the specified format.
    /// </summary>
    [Combinator]
    [DefaultProperty(nameof(CompressionParameters))]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Encodes each image in the sequence into a byte buffer using the specified format.")]
    public class EncodeImage
    {
        readonly CompressionParameterCollection compressionParameters = new CompressionParameterCollection();

        /// <summary>
        /// Gets or sets the file extension that defines the encoding format.
        /// </summary>
        [TypeConverter(typeof(ExtensionConverter))]
        [Description("The file extension that defines the encoding format.")]
        public string Extension { get; set; } = "jpg";

        /// <summary>
        /// Gets the collection of optional image compression parameters.
        /// </summary>
        [XmlArrayItem("Parameter")]
        [Description("The optional image compression parameters.")]
        [TypeConverter(typeof(CompressionParameterCollectionConverter))]
        [Editor("Bonsai.Vision.Design.CompressionParameterCollectionEditor, Bonsai.Vision.Design", DesignTypes.UITypeEditor)]
        public CompressionParameterCollection CompressionParameters
        {
            get { return compressionParameters; }
        }

        /// <summary>
        /// Encodes each image in an observable sequence into a byte buffer in memory
        /// using the specified format.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence of array-like objects to be encoded.
        /// </param>
        /// <returns>
        /// The sequence of encoded memory buffers.
        /// </returns>
        public IObservable<Mat> Process<TArray>(IObservable<TArray> source) where TArray : Arr
        {
            return Observable.Defer(() =>
            {
                var extension = Extension;
                if (!extension.StartsWith(".")) extension = "." + extension;
                var parameters = CompressionParameters.GetParameters();
                return source.Select(input => CV.EncodeImage(extension, input, parameters));
            });
        }

        class ExtensionConverter : StringConverter
        {
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                return new StandardValuesCollection(new[]
                {
                    "jpg",
                    "png",
                    "bmp",
                    "ppm",
                    "pgm",
                    "pbm"
                });
            }
        }

        class CompressionParameterCollectionConverter : CollectionConverter
        {
            public override bool GetPropertiesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
            {
                var parameterCollection = (CompressionParameterCollection)value;
                if (parameterCollection != null && parameterCollection.Count > 0)
                {
                    var properties = (from parameter in parameterCollection
                                      let valueProperty = TypeDescriptor.GetDefaultProperty(parameter)
                                      where valueProperty != null
                                      select new ParameterPropertyDescriptor(parameterCollection, parameter, valueProperty))
                                     .ToArray();
                    return new PropertyDescriptorCollection(properties);
                }

                return base.GetProperties(context, value, attributes);
            }

            class ParameterPropertyDescriptor : PropertyDescriptor
            {
                public ParameterPropertyDescriptor(CompressionParameterCollection parameterCollection, CompressionParameterAssignment parameter, PropertyDescriptor value)
                    : base(parameter.GetType().Name, value.Attributes.Cast<Attribute>().ToArray())
                {
                    ParameterCollection = parameterCollection;
                    Parameter = parameter;
                    Value = value;
                }

                public CompressionParameterCollection ParameterCollection { get; private set; }

                public object Parameter { get; private set; }

                public PropertyDescriptor Value { get; private set; }

                public override Type ComponentType => Parameter.GetType();

                public override bool IsReadOnly => Value.IsReadOnly;

                public override Type PropertyType => Value.PropertyType;

                public override bool CanResetValue(object component)
                {
                    return Value.CanResetValue(Parameter);
                }

                public override object GetValue(object component)
                {
                    return Value.GetValue(Parameter);
                }

                public override void ResetValue(object component)
                {
                    Value.ResetValue(Parameter);
                }

                public override void SetValue(object component, object value)
                {
                    Value.SetValue(Parameter, value);
                }

                public override bool ShouldSerializeValue(object component)
                {
                    return Value.ShouldSerializeValue(Parameter);
                }
            }
        }
    }
}

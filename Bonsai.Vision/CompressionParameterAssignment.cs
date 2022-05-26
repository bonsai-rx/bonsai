using System.ComponentModel;
using System.Xml.Serialization;
using OpenCV.Net;

namespace Bonsai.Vision
{
    /// <summary>
    /// Provides an abstract base class for configuring various image
    /// compression parameters.
    /// </summary>
    [DefaultProperty("Value")]
    [XmlInclude(typeof(JpegQuality))]
    [XmlInclude(typeof(PngCompressionLevel))]
    [XmlInclude(typeof(PngCompressionStrategy))]
    [XmlInclude(typeof(PngBiLevelCompression))]
    [XmlInclude(typeof(PxmBinaryFormat))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    public abstract class CompressionParameterAssignment
    {
        internal abstract int ParameterType { get; }

        internal abstract int ParameterValue { get; }

        /// <summary>
        /// Creates a <see cref="string"/> representation of the
        /// compression parameter.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> containing the parameter name and the value
        /// to be assigned by this object.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0} = {1}", GetType().Name, ParameterValue);
        }
    }

    /// <summary>
    /// Provides a class for specifying the quality of image JPEG compression.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class JpegQuality : CompressionParameterAssignment
    {
        /// <summary>
        /// Gets or sets a value specifying the quality of image JPEG compression
        /// from 0 to 100 (the higher the better). Default value is 95.
        /// </summary>
        [XmlAttribute]
        [Range(0, 100)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("Specifies the quality of image JPEG compression from 0 to 100 (the higher the better).")]
        public int Value { get; set; } = 95;

        internal override int ParameterType => CompressionParameters.JpegQuality;

        internal override int ParameterValue => Value;
    }

    /// <summary>
    /// Provides a class for specifying the PNG compression level.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class PngCompressionLevel : CompressionParameterAssignment
    {
        /// <summary>
        /// Gets or sets a value specifying the PNG compression level from 0 to 9.
        /// A higher value means a smaller size and longer compression time.
        /// Default value is 3.
        /// </summary>
        [Range(0, 9)]
        [XmlAttribute]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("Specifies the the PNG compression level from 0 to 9. A higher value means a smaller size and longer compression time.")]
        public int Value { get; set; } = 3;

        internal override int ParameterType => CompressionParameters.PngCompression;

        internal override int ParameterValue => Value;
    }

    /// <summary>
    /// Provides a class for specifying the PNG compression strategy.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class PngCompressionStrategy : CompressionParameterAssignment
    {
        /// <summary>
        /// Gets or sets a value specifying the PNG compression strategy.
        /// </summary>
        [XmlAttribute]
        [Description("Specifies the PNG compression strategy.")]
        public PngCompression Value { get; set; }

        internal override int ParameterType => CompressionParameters.PngStrategy;

        internal override int ParameterValue => (int)Value;
    }

    /// <summary>
    /// Provides a class for specifying whether PNG bi-level compression.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class PngBiLevelCompression : CompressionParameterAssignment
    {
        /// <summary>
        /// Gets or sets a value indicating whether PNG compression should use
        /// bi-level (binary) images.
        /// </summary>
        [XmlAttribute]
        [Description("Specifies whether PNG compression should use bi-level (binary) images.")]
        public bool Value { get; set; }

        internal override int ParameterType => CompressionParameters.PngBiLevel;

        internal override int ParameterValue => Value ? 1 : 0;
    }

    /// <summary>
    /// Provides a class for specifying a binary format flag for PPM, PGM or PBM.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class PxmBinaryFormat : CompressionParameterAssignment
    {
        /// <summary>
        /// Gets or sets a value specifying a binary format flag for PPM, PGM or PBM.
        /// Default value is 1.
        /// </summary>
        [XmlAttribute]
        [Description("Specifies a binary format flag for PPM, PGM or PBM.")]
        public int Value { get; set; }

        internal override int ParameterType => CompressionParameters.PxmBinary;

        internal override int ParameterValue => Value;
    }

    /// <summary>
    /// Specifies the available PNG compression strategies.
    /// </summary>
    public enum PngCompression : int
    {
        /// <summary>
        /// Specifies the default PNG compression strategy.
        /// </summary>
        Default = CompressionParameters.PngStrategyDefault,

        /// <summary>
        /// Specifies a filtered PNG compression strategy.
        /// </summary>
        Filtered = CompressionParameters.PngStrategyFiltered,

        /// <summary>
        /// Specifies a huffman code based PNG compression strategy.
        /// </summary>
        HuffmanOnly = CompressionParameters.PngStrategyHuffmanOnly,

        /// <summary>
        /// Specifies a run-length encoding PNG compression strategy.
        /// </summary>
        RunLengthEncoding = CompressionParameters.PngStrategyRle,

        /// <summary>
        /// Specifies a fixed PNG compression strategy.
        /// </summary>
        Fixed = CompressionParameters.PngStrategyFixed
    }
}

using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Provides an abstract base class for configuring the state of the current
    /// graphics context.
    /// </summary>
    [XmlInclude(typeof(HintState))]
    [XmlInclude(typeof(EnableState))]
    [XmlInclude(typeof(DisableState))]
    [XmlInclude(typeof(ViewportState))]
    [XmlInclude(typeof(ScissorState))]
    [XmlInclude(typeof(ClearColorState))]
    [XmlInclude(typeof(LineWidthState))]
    [XmlInclude(typeof(PointSizeState))]
    [XmlInclude(typeof(DepthMaskState))]
    [XmlInclude(typeof(PolygonModeState))]
    [XmlInclude(typeof(BlendFunctionState))]
    [XmlInclude(typeof(DepthFunctionState))]
    [XmlInclude(typeof(MemoryBarrierState))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    public abstract class StateConfiguration
    {
        /// <summary>
        /// Updates the state of the current graphics context.
        /// </summary>
        /// <param name="window">
        /// The shader window associated with the current graphics context.
        /// </param>
        public abstract void Execute(ShaderWindow window);
    }
}

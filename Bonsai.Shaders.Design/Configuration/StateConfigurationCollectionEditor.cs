using Bonsai.Resources.Design;
using System;

namespace Bonsai.Shaders.Configuration.Design
{
    /// <summary>
    /// Provides a user interface editor that displays a dialog for editing a
    /// collection of render state configuration objects.
    /// </summary>
    public class StateConfigurationCollectionEditor : CollectionEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StateConfigurationCollectionEditor"/>
        /// class using the specified collection type.
        /// </summary>
        /// <param name="type">
        /// The type of the collection for this editor to edit.
        /// </param>
        public StateConfigurationCollectionEditor(Type type)
            : base(type)
        {
        }

        /// <inheritdoc/>
        protected override Type[] CreateNewItemTypes()
        {
            return new[]
            {
                typeof(EnableState),
                typeof(DisableState),
                typeof(ViewportState),
                typeof(ScissorState),
                typeof(ClearColorState),
                typeof(LineWidthState),
                typeof(PointSizeState),
                typeof(DepthMaskState),
                typeof(PolygonModeState),
                typeof(BlendFunctionState),
                typeof(DepthFunctionState),
                typeof(MemoryBarrierState),
                typeof(HintState)
            };
        }
    }
}

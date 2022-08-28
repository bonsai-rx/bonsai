using Bonsai.Resources.Design;
using System;

namespace Bonsai.Shaders.Configuration.Design
{
    /// <summary>
    /// Provides a user interface editor that displays a dialog for editing a
    /// collection of shader uniform configuration objects.
    /// </summary>
    public class UniformConfigurationCollectionEditor : CollectionEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UniformConfigurationCollectionEditor"/>
        /// class using the specified collection type.
        /// </summary>
        /// <param name="type">
        /// The type of the collection for this editor to edit.
        /// </param>
        public UniformConfigurationCollectionEditor(Type type)
            : base(type)
        {
        }

        /// <inheritdoc/>
        protected override string GetDisplayText(object value)
        {
            return value.ToString();
        }

        /// <inheritdoc/>
        protected override Type[] CreateNewItemTypes()
        {
            return new[]
            {
                typeof(FloatUniform),
                typeof(Vec2Uniform),
                typeof(Vec3Uniform),
                typeof(Vec4Uniform)
            };
        }
    }
}

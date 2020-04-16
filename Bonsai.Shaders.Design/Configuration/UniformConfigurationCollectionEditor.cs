using Bonsai.Design;
using System;

namespace Bonsai.Shaders.Configuration.Design
{
    public class UniformConfigurationCollectionEditor : DescriptiveCollectionEditor
    {
        public UniformConfigurationCollectionEditor(Type type)
            : base(type)
        {
        }

        protected override string GetDisplayText(object value)
        {
            return value.ToString();
        }

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

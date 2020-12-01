using Assimp;
using Bonsai.Resources;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Bonsai.Shaders.Rendering
{
    [DefaultProperty(nameof(Scenes))]
    [Description("Creates a collection of scene resources to be loaded into the resource manager.")]
    public class SceneResources : ResourceLoader
    {
        const string PostProcessingCategory = "Post Processing";
        readonly SceneConfigurationCollection scenes = new SceneConfigurationCollection();

        public SceneResources()
        {
            PostProcessSteps = PostProcessSteps.Triangulate;
            Scale = 1;
        }

        [Editor("Bonsai.Resources.Design.ResourceCollectionEditor, Bonsai.System.Design", DesignTypes.UITypeEditor)]
        [Description("The collection of scene resources to be loaded into the resource manager.")]
        public SceneConfigurationCollection Scenes
        {
            get { return scenes; }
        }

        [TypeConverter(typeof(ShaderNameConverter))]
        [Description("The name of the shader program used to render scene materials.")]
        public string ShaderName { get; set; }

        [Category(PostProcessingCategory)]
        [Description("The optional post processing steps that can be run on the data to generate or optimize vertex data.")]
        public PostProcessSteps PostProcessSteps { get; set; }

        [Category(PostProcessingCategory)]
        [Description("The optional uniform scale factor to apply to the model transform nodes.")]
        public float Scale { get; set; }

        [Category(PostProcessingCategory)]
        [Description("The optional model rotation about the X-axis. This is used during the loading stage only.")]
        public float RotationX { get; set; }

        [Category(PostProcessingCategory)]
        [Description("The optional model rotation about the Y-axis. This is used during the loading stage only.")]
        public float RotationY { get; set; }

        [Category(PostProcessingCategory)]
        [Description("The optional model rotation about the Z-axis. This is used during the loading stage only.")]
        public float RotationZ { get; set; }

        class SceneRendererConfiguration : ResourceConfiguration<ISceneRenderer>
        {
            public SceneResources Configuration { get; set; }

            public string FileName { get; set; }

            public override ISceneRenderer CreateResource(ResourceManager resourceManager)
            {
                var shader = resourceManager.Load<Shader>(Configuration.ShaderName);
                using (var context = new AssimpContext())
                {
                    context.Scale = Configuration.Scale;
                    context.XAxisRotation = Configuration.RotationX;
                    context.YAxisRotation = Configuration.RotationY;
                    context.ZAxisRotation = Configuration.RotationZ;
                    var scene = context.ImportFile(FileName, Configuration.PostProcessSteps);
                    return new SceneRenderer(shader, resourceManager, scene);
                }
            }
        }

        protected override IEnumerable<IResourceConfiguration> GetResources()
        {
            return scenes.Select(scene => new SceneRendererConfiguration
            {
                Configuration = this,
                FileName = scene.FileName,
                Name = scene.Name
            });
        }
    }
}

using Assimp;
using Bonsai.Resources;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Bonsai.Shaders.Rendering
{
    /// <summary>
    /// Represents an operator that creates a collection of scene resources to
    /// be loaded into the resource manager.
    /// </summary>
    [DefaultProperty(nameof(Scenes))]
    [Description("Creates a collection of scene resources to be loaded into the resource manager.")]
    public class SceneResources : ResourceLoader
    {
        const string PostProcessingCategory = "Post Processing";
        readonly SceneConfigurationCollection scenes = new SceneConfigurationCollection();

        /// <summary>
        /// Gets the collection of scene resources to be loaded into the resource manager.
        /// </summary>
        [Editor("Bonsai.Resources.Design.ResourceCollectionEditor, Bonsai.System.Design", DesignTypes.UITypeEditor)]
        [Description("The collection of scene resources to be loaded into the resource manager.")]
        public SceneConfigurationCollection Scenes
        {
            get { return scenes; }
        }

        /// <summary>
        /// Gets or sets the name of the shader program used to render scene materials.
        /// </summary>
        [TypeConverter(typeof(ShaderNameConverter))]
        [Description("The name of the shader program used to render scene materials.")]
        public string ShaderName { get; set; }

        /// <summary>
        /// Gets or sets a value specifying post processing steps to run on the data
        /// for generating or optimizing vertex data.
        /// </summary>
        [Category(PostProcessingCategory)]
        [Description("Specifies post processing steps to run on the data for generating or optimizing vertex data.")]
        public PostProcessSteps PostProcessSteps { get; set; } = PostProcessSteps.Triangulate;

        /// <summary>
        /// Gets or sets the uniform scale factor to apply to the model transform nodes.
        /// </summary>
        [Category(PostProcessingCategory)]
        [Description("The uniform scale factor to apply to the model transform nodes.")]
        public float Scale { get; set; } = 1;

        /// <summary>
        /// Gets or sets the model rotation about the X-axis. This property is only used
        /// during the loading stage.
        /// </summary>
        [Category(PostProcessingCategory)]
        [Description("The model rotation about the X-axis. This property is only used during the loading stage.")]
        public float RotationX { get; set; }

        /// <summary>
        /// Gets or sets the model rotation about the Y-axis. This property is only used
        /// during the loading stage.
        /// </summary>
        [Category(PostProcessingCategory)]
        [Description("The model rotation about the Y-axis. This property is only used during the loading stage.")]
        public float RotationY { get; set; }

        /// <summary>
        /// Gets or sets the model rotation about the Z-axis. This property is only used
        /// during the loading stage.
        /// </summary>
        [Category(PostProcessingCategory)]
        [Description("The model rotation about the Z-axis. This property is only used during the loading stage.")]
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

        /// <inheritdoc/>
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

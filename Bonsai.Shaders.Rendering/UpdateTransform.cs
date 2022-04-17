using OpenTK;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders.Rendering
{
    /// <summary>
    /// Represents an operator that updates the transform matrix of the
    /// specified scene node.
    /// </summary>
    [Description("Updates the transform matrix of the specified scene node.")]
    public class UpdateTransform : Sink<Matrix4>
    {
        /// <summary>
        /// Gets or sets the name of the scene to update.
        /// </summary>
        [TypeConverter(typeof(SceneNameConverter))]
        [Description("The name of the scene to update.")]
        public string SceneName { get; set; }

        /// <summary>
        /// Gets or sets the name of the scene node to update. If no name is
        /// specified, the root node will be updated.
        /// </summary>
        [Description("The name of the scene node to update. If no name is specified, the root node will be updated.")]
        public string NodeName { get; set; }

        /// <summary>
        /// Updates the transform matrix of the specified scene node using each
        /// of the matrix values in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="Matrix4"/> objects representing the transform
        /// matrix used to render the scene node.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of updating the
        /// transform matrix used to render the scene node.
        /// </returns>
        public override IObservable<Matrix4> Process(IObservable<Matrix4> source)
        {
            return ShaderManager.WindowSource.SelectMany(window =>
            {
                var node = default(SceneNode);
                var scene = default(ISceneRenderer);
                var sceneName = default(string);
                var nodeName = default(string);
                return source.Do(value =>
                {
                    if (sceneName != SceneName)
                    {
                        nodeName = null;
                        sceneName = SceneName;
                        scene = !string.IsNullOrEmpty(sceneName) ? window.ResourceManager.Load<ISceneRenderer>(sceneName) : null;
                        node = scene.RootNode;
                    }

                    if (scene != null)
                    {
                        if (nodeName != NodeName)
                        {
                            nodeName = NodeName;
                            if (string.IsNullOrEmpty(nodeName)) node = scene.RootNode;
                            else node = scene.RootNode.FindNode(nodeName) ?? throw new InvalidOperationException(
                                string.Format("The node with name '{0}' was not found.", nodeName));
                        }

                        node.Transform = value;
                    }
                });
            });
        }
    }
}

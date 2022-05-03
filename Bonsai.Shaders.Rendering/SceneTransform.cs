using OpenTK;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace Bonsai.Shaders.Rendering
{
    /// <summary>
    /// Represents an operator that gets the transform matrix of the specified
    /// scene node.
    /// </summary>
    [Description("Gets the transform matrix of the specified scene node.")]
    public class SceneTransform : Source<Matrix4>
    {
        /// <summary>
        /// Gets or sets the name of the scene where the node is located.
        /// </summary>
        [TypeConverter(typeof(SceneNameConverter))]
        [Description("The name of the scene where the node is located.")]
        public string SceneName { get; set; }

        /// <summary>
        /// Gets or sets the name of the scene node to get the transform from.
        /// If no name is specified, the root node will be used.
        /// </summary>
        [Description("The name of the scene node to get the transform from. If no name is specified, the root node will be used.")]
        public string NodeName { get; set; }

        /// <summary>
        /// Gets the transform matrix of the specified scene node and surfaces
        /// it through an observable sequence.
        /// </summary>
        /// <returns>
        /// A sequence containing the <see cref="Matrix4"/> object representing
        /// the transform of the specified scene node.
        /// </returns>
        public override IObservable<Matrix4> Generate()
        {
            return Generate(Observable.Return(Unit.Default));
        }

        /// <summary>
        /// Gets the transform matrix of the specified scene node whenever an
        /// observable sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence of notifications used to extract the node transform.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Matrix4"/> objects representing the transform
        /// of the specified scene node.
        /// </returns>
        public IObservable<Matrix4> Generate<TSource>(IObservable<TSource> source)
        {
            return ShaderManager.WindowSource.SelectMany(window =>
            {
                var node = default(SceneNode);
                var scene = default(ISceneRenderer);
                var sceneName = default(string);
                var nodeName = default(string);
                return source.Select(value =>
                {
                    if (sceneName != SceneName)
                    {
                        nodeName = null;
                        sceneName = SceneName;
                        scene = window.ResourceManager.Load<ISceneRenderer>(sceneName);
                        node = scene.RootNode;
                    }

                    if (nodeName != NodeName)
                    {
                        nodeName = NodeName;
                        if (string.IsNullOrEmpty(nodeName)) node = scene.RootNode;
                        else node = scene.RootNode.FindNode(nodeName) ?? throw new InvalidOperationException(
                            string.Format("The node with name '{0}' was not found.", nodeName));
                    }

                    return node.Transform;
                });
            });
        }
    }
}

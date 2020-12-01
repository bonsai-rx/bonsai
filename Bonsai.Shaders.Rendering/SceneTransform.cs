using OpenTK;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace Bonsai.Shaders.Rendering
{
    [Description("Gets the transform matrix of the specified scene node.")]
    public class SceneTransform : Source<Matrix4>
    {
        [TypeConverter(typeof(SceneNameConverter))]
        [Description("The name of the scene where the node is located.")]
        public string SceneName { get; set; }

        [Description("The name of the scene node to get the transform from. If no name is specified, the root node will be used.")]
        public string NodeName { get; set; }

        public override IObservable<Matrix4> Generate()
        {
            return Generate(Observable.Return(Unit.Default));
        }

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

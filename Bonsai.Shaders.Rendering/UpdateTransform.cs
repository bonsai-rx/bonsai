using OpenTK;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders.Rendering
{
    [Description("Updates the transform matrix of the specified scene node.")]
    public class UpdateTransform : Sink<Matrix4>
    {
        [TypeConverter(typeof(SceneNameConverter))]
        [Description("The name of the scene to update.")]
        public string SceneName { get; set; }

        [Description("The name of the scene node to update. If no name is specified, the root node will be updated.")]
        public string NodeName { get; set; }

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

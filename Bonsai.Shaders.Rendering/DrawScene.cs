using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders.Rendering
{
    [Description("Draws the specified scene. Each scene is assigned to a specific renderer which controls the rendering steps.")]
    public class DrawScene : Sink
    {
        [TypeConverter(typeof(SceneNameConverter))]
        [Description("The name of the scene to draw.")]
        public string SceneName { get; set; }

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return ShaderManager.WindowSource.SelectMany(window =>
            {
                var scene = default(ISceneRenderer);
                var sceneName = default(string);
                return source.Do(x =>
                {
                    if (sceneName != SceneName)
                    {
                        sceneName = SceneName;
                        scene = !string.IsNullOrEmpty(sceneName) ? window.ResourceManager.Load<ISceneRenderer>(sceneName) : null;
                    }

                    if (scene != null)
                    {
                        scene.Draw();
                    }
                });
            });
        }
    }
}

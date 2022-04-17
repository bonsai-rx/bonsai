using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders.Rendering
{
    /// <summary>
    /// Represents an operator that draws the specified scene.
    /// </summary>
    /// <remarks>
    /// Each scene is assigned to a specific renderer which controls the rendering steps.
    /// </remarks>
    [Description("Draws the specified scene. Each scene is assigned to a specific renderer which controls the rendering steps.")]
    public class DrawScene : Sink
    {
        /// <summary>
        /// Gets or sets the name of the scene to draw.
        /// </summary>
        [TypeConverter(typeof(SceneNameConverter))]
        [Description("The name of the scene to draw.")]
        public string SceneName { get; set; }

        /// <summary>
        /// Draws the specified scene whenever an observable sequence emits
        /// a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements of the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence of notifications used to start drawing the scene. 
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of drawing the
        /// specified scene whenever the sequence emits a notification.
        /// </returns>
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

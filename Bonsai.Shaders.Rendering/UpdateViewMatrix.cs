using OpenTK;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders.Rendering
{
    [Description("Updates the view matrix used to render the specified scene.")]
    public class UpdateViewMatrix : Sink
    {
        [TypeConverter(typeof(SceneNameConverter))]
        [Description("The name of the scene to update.")]
        public string SceneName { get; set; }

        [Description("The name of the camera used to render the scene. If a view matrix is specified as input, this property is optional.")]
        public string CameraName { get; set; }

        IObservable<TSource> Process<TSource>(IObservable<TSource> source, Action<SceneRenderer, TSource> update)
        {
            return ShaderManager.WindowSource.SelectMany(window =>
            {
                var camera = default(SceneCamera);
                var scene = default(SceneRenderer);
                var sceneName = default(string);
                var cameraName = default(string);
                return source.Do(value =>
                {
                    if (sceneName != SceneName)
                    {
                        camera = null;
                        cameraName = null;
                        sceneName = SceneName;
                        scene = !string.IsNullOrEmpty(sceneName) ? (SceneRenderer)window.ResourceManager.Load<ISceneRenderer>(sceneName) : null;
                    }

                    if (scene != null)
                    {
                        if (cameraName != CameraName)
                        {
                            cameraName = CameraName;
                            if (string.IsNullOrEmpty(cameraName)) camera = null;
                            else camera = scene.FindCamera(cameraName) ?? throw new InvalidOperationException(
                                string.Format("The camera with name '{0}' was not found.", cameraName));
                        }

                        if (camera != null) scene.ViewMatrix = camera.ViewMatrix;
                        else update(scene, value);
                    }
                });
            });
        }

        public IObservable<Matrix4> Process(IObservable<Matrix4> source)
        {
            return Process(source, (scene, value) => scene.ViewMatrix = value);
        }

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return Process(source, (scene, value) => throw new InvalidOperationException("No valid camera name was specified."));
        }
    }
}

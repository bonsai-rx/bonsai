using OpenTK;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders.Rendering
{
    /// <summary>
    /// Represents an operator that updates the projection matrix used to render
    /// the specified scene.
    /// </summary>
    [Description("Updates the projection matrix used to render the specified scene.")]
    public class UpdateProjectionMatrix : Sink
    {
        /// <summary>
        /// Gets or sets the name of the scene to update.
        /// </summary>
        [TypeConverter(typeof(SceneNameConverter))]
        [Description("The name of the scene to update.")]
        public string SceneName { get; set; }

        /// <summary>
        /// Gets or sets the name of the camera used to render the scene.
        /// </summary>
        /// <remarks>
        /// If a sequence of projection matrices is provided to the operator, this
        /// property is optional.
        /// </remarks>
        [Description("The name of the camera used to render the scene. If a sequence of projection matrices is provided to the operator, this property is optional.")]
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

                        if (camera != null) scene.ProjectionMatrix = camera.ProjectionMatrix;
                        else update(scene, value);
                    }
                });
            });
        }

        /// <summary>
        /// Updates the projection matrix used to render the scene using each of
        /// the matrix values in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="Matrix4"/> objects representing the projection
        /// matrix used to render the scene, if no camera is specified.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of updating the
        /// projection matrix used to render the scene.
        /// </returns>
        public IObservable<Matrix4> Process(IObservable<Matrix4> source)
        {
            return Process(source, (scene, value) => scene.ProjectionMatrix = value);
        }

        /// <summary>
        /// Updates the projection matrix used to render the scene whenever an
        /// observable sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence of notifications used to update the projection matrix.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of updating the
        /// projection matrix used to render the scene whenever the sequence emits
        /// a notification.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return Process(source, (scene, value) => throw new InvalidOperationException("No valid camera name was specified."));
        }
    }
}

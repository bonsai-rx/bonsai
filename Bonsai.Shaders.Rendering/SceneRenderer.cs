using Assimp;
using Bonsai.Resources;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;

namespace Bonsai.Shaders.Rendering
{
    class SceneRenderer : ISceneRenderer
    {
        readonly List<SceneMesh> meshes = new List<SceneMesh>();
        readonly List<MaterialBinding> materials = new List<MaterialBinding>();
        readonly List<SceneCamera> cameras = new List<SceneCamera>();
        readonly Action<Matrix4> setModelViewMatrix = _ => { };
        readonly Action<Matrix4> setProjectionMatrix = _ => { };
        readonly Action draw;

        internal SceneRenderer(Shader shader, ResourceManager resourceManager, Assimp.Scene scene)
        {
            Shader = shader ?? throw new ArgumentNullException(nameof(shader));
            foreach (var resource in scene.Meshes)
            {
                var mesh = SceneMeshFactory.CreateMesh(resource);
                meshes.Add(mesh);
            }

            foreach (var material in scene.Materials)
            {
                materials.Add(new MaterialBinding(shader, resourceManager, material));
            }

            var projectionMatrixLocation = GL.GetUniformLocation(shader.Program, ShaderConstants.ProjectionMatrix);
            if (projectionMatrixLocation >= 0)
            {
                setProjectionMatrix = projection => GL.UniformMatrix4(projectionMatrixLocation, transpose: false, ref projection);
            }

            var modelViewMatrixLocation = GL.GetUniformLocation(shader.Program, ShaderConstants.ModelViewMatrix);
            if (modelViewMatrixLocation >= 0)
            {
                setModelViewMatrix = transform =>
                {
                    var modelViewMatrix = ViewMatrix;
                    Matrix4.Mult(ref transform, ref modelViewMatrix, out modelViewMatrix);
                    GL.UniformMatrix4(modelViewMatrixLocation, transpose: false, ref modelViewMatrix);
                };
            }

            draw = () =>
            {
                setProjectionMatrix(ProjectionMatrix);
                Draw(RootNode);
            };

            RootNode = new SceneNode(scene.RootNode, Matrix4x4.Identity, meshes, setModelViewMatrix, index => materials[index]);
            foreach (var camera in scene.Cameras)
            {
                var node = RootNode.FindNode(camera.Name);
                if (node != null)
                {
                    cameras.Add(new SceneCamera(camera, shader.Window, node));
                }
            }
        }

        private Shader Shader { get; set; }

        internal Matrix4 ViewMatrix { get; set; }

        internal Matrix4 ProjectionMatrix { get; set; }

        public SceneNode RootNode { get; private set; }

        internal SceneCamera FindCamera(string name)
        {
            return cameras.Find(camera => string.Equals(camera.Name, name));
        }

        public void Draw()
        {
            Shader.Update(draw);
        }

        void Draw(SceneNode node)
        {
            node.Draw();
            foreach (var child in node.Children)
            {
                Draw(child);
            }
        }

        public void Dispose()
        {
            meshes.RemoveAll(mesh => { mesh.Dispose(); return true; });
            materials.Clear();
        }
    }
}

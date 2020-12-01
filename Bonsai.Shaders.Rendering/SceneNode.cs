using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bonsai.Shaders.Rendering
{
    public class SceneNode
    {
        static readonly Action EmptyAction = () => { };
        readonly List<SceneNode> children = new List<SceneNode>();
        readonly Action draw;

        internal SceneNode(Assimp.Node node, Assimp.Matrix4x4 transform, List<SceneMesh> meshes, Action<Matrix4> setTransform, Func<int, MaterialBinding> materialSelector)
        {
            Name = node.Name;
            draw = EmptyAction;
            Transform = MatrixHelper.ToMatrix4(transform * node.Transform);
            if (node.HasMeshes)
            {
                draw += () => setTransform(Transform);
                foreach (var index in node.MeshIndices)
                {
                    var mesh = meshes[index];
                    draw += () =>
                    {
                        var material = materialSelector(mesh.MaterialIndex);
                        if (material != null)
                        {
                            material.Bind();
                            mesh.Draw();
                        }
                    };
                }
            }

            if (node.HasChildren)
            {
                children.AddRange(node.Children.Select(
                    child => new SceneNode(child, node.Transform, meshes, setTransform, materialSelector)));
            }
        }

        public string Name { get; private set; }

        public Matrix4 Transform { get; set; }

        public IEnumerable<SceneNode> Children
        {
            get { return children; }
        }

        public SceneNode FindNode(string name)
        {
            if (string.Equals(name, Name)) return this;
            if (children.Count > 0)
            {
                foreach (var child in children)
                {
                    var found = child.FindNode(name);
                    if (found != null) return found;
                }
            }

            return null;
        }

        internal void Draw()
        {
            draw();
        }
    }
}

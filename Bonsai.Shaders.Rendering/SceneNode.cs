using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bonsai.Shaders.Rendering
{
    /// <summary>
    /// Represents a node in the scene graph containing a transform and a set
    /// of mesh geometries to render.
    /// </summary>
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

        /// <summary>
        /// Gets the name of the scene node.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the transform matrix specifying the rotation, scale,
        /// and position of the scene node.
        /// </summary>
        public Matrix4 Transform { get; set; }

        /// <summary>
        /// Gets the collection of children of this scene node.
        /// </summary>
        public IEnumerable<SceneNode> Children
        {
            get { return children; }
        }

        /// <summary>
        /// Searches the hierarchy for a scene node with the specified name. 
        /// </summary>
        /// <param name="name">
        /// The name of the scene node to find.
        /// </param>
        /// <returns>
        /// A <see cref="SceneNode"/> object which is either the current node,
        /// or one of its children, that matches the specified name; or
        /// <see langword="null"/> if no matching node is found.
        /// </returns>
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

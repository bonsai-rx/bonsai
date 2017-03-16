﻿using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Description("Aggregates the specified mesh geometries into a single draw command.")]
    public class DrawMeshAggregate : Sink
    {
        readonly Collection<MeshName> meshNames = new Collection<MeshName>();

        [Description("The name of the material shader program.")]
        [Editor("Bonsai.Shaders.Configuration.Design.MaterialConfigurationEditor, Bonsai.Shaders.Design", typeof(UITypeEditor))]
        public string ShaderName { get; set; }

        [Description("The name of the mesh geometry to draw.")]
        public Collection<MeshName> MeshNames
        {
            get { return meshNames; }
        }

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return Observable.Defer(() =>
            {
                if (meshNames.Count == 0)
                {
                    throw new InvalidOperationException("A mesh name must be specified.");
                }

                MeshAggregate mesh = null;
                return source.CombineEither(
                    ShaderManager.ReserveMaterial(ShaderName).Do(material =>
                    {
                        material.Update(() =>
                        {
                            var meshAttributes = meshNames.Select(meshName => new MeshAttributeMapping(
                                material.Window.Meshes[meshName.Name],
                                meshName.Divisor));
                            mesh = new MeshAggregate(meshAttributes);
                        });
                    }),
                    (input, material) =>
                    {
                        material.Update(() =>
                        {
                            mesh.Draw();
                        });
                        return input;
                    }).Finally(() =>
                    {
                        if (mesh != null)
                        {
                            mesh.Dispose();
                        }
                    });
            });
        }
    }
}

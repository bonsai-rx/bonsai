﻿using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Sink)]
    [Description("Draws the specified mesh geometry.")]
    public class DrawMesh
    {
        [TypeConverter(typeof(MaterialNameConverter))]
        [Description("The name of the material shader program.")]
        public string ShaderName { get; set; }

        [TypeConverter(typeof(MeshNameConverter))]
        [Description("The name of the mesh geometry to draw.")]
        public string MeshName { get; set; }

        public IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return Observable.Create<TSource>(observer =>
            {
                var drawMesh = default(Action);
                var meshName = default(string);
                return source.CombineEither(
                    ShaderManager.ReserveShader(ShaderName),
                    (input, shader) =>
                    {
                        if (meshName != MeshName)
                        {
                            meshName = MeshName;
                            if (!string.IsNullOrEmpty(meshName))
                            {
                                var mesh = shader.Window.ResourceManager.Load<Mesh>(meshName);
                                drawMesh = () => mesh.Draw();
                            }
                            else drawMesh = null;
                        }

                        shader.Update(drawMesh);
                        return input;
                    }).SubscribeSafe(observer);
            });
        }
    }
}

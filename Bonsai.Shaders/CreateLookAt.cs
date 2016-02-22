﻿using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public class CreateLookAt : Source<Matrix4>
    {
        Vector3 eye;
        Vector3 target;
        Vector3 up;

        public CreateLookAt()
        {
            up = Vector3.UnitY;
        }

        [TypeConverter("OpenCV.Net.NumericAggregateConverter, OpenCV.Net")]
        public Vector3 Eye
        {
            get { return eye; }
            set { eye = value; }
        }

        [TypeConverter("OpenCV.Net.NumericAggregateConverter, OpenCV.Net")]
        public Vector3 Target
        {
            get { return target; }
            set { target = value; }
        }

        [TypeConverter("OpenCV.Net.NumericAggregateConverter, OpenCV.Net")]
        public Vector3 Up
        {
            get { return up; }
            set { up = value; }
        }

        public override IObservable<Matrix4> Generate()
        {
            return Observable.Defer(() => Observable.Return(Matrix4.LookAt(
                eye.X, eye.Y, eye.Z,
                target.X, target.Y, target.Z,
                up.X, up.Y, up.Z)));
        }

        public IObservable<Matrix4> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => Matrix4.LookAt(
                eye.X, eye.Y, eye.Z,
                target.X, target.Y, target.Z,
                up.X, up.Y, up.Z));
        }
    }
}

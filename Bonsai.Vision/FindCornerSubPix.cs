using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Vision
{
    [Description("Finds the sub-pixel accurate location of corners or radial saddle points.")]
    public class FindCornerSubPix : Transform<KeyPointCollection, KeyPointCollection>
    {
        public FindCornerSubPix()
        {
            WindowSize = new Size(15, 15);
            ZeroZone = new Size(-1, -1);
            MaxIterations = 20;
            Epsilon = 0.01;
        }

        [Description("Half of the side length of the corner search window.")]
        public Size WindowSize { get; set; }

        [Description("Half of the side length of the middle search window that will be ignored during refinement.")]
        public Size ZeroZone { get; set; }

        [Description("The maximum number of iterations.")]
        public int MaxIterations { get; set; }

        [Description("The minimum required accuracy for convergence.")]
        public double Epsilon { get; set; }

        public override IObservable<KeyPointCollection> Process(IObservable<KeyPointCollection> source)
        {
            return source.Select(input =>
            {
                var corners = input.ToArray();
                var maxIterations = MaxIterations;
                var epsilon = Epsilon;
                var terminationType = TermCriteriaType.None;
                if (maxIterations > 0) terminationType |= TermCriteriaType.MaxIter;
                if (epsilon > 0) terminationType |= TermCriteriaType.Epsilon;
                var termCriteria = new TermCriteria(terminationType, maxIterations, epsilon);
                CV.FindCornerSubPix(input.Image, corners, WindowSize, ZeroZone, termCriteria);

                var result = new KeyPointCollection(input.Image);
                for (int i = 0; i < corners.Length; i++)
                {
                    result.Add(corners[i]);
                }
                return result;
            });
        }
    }
}

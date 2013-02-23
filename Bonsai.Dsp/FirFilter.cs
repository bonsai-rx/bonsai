﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Dsp
{
    public class FirFilter : Transform<CvMat, CvMat>
    {
        CvMat kernel;
        CvMat overlap;
        CvMat overlapInput;
        CvMat overlapEnd;
        CvMat overlapStart;
        CvRect overlapOutput;
        float[] currentKernel;

        public FirFilter()
        {
            Anchor = -1;
        }

        public int Anchor { get; set; }

        [XmlIgnore]
        [TypeConverter(typeof(UnidimensionalArrayConverter))]
        public float[] Kernel { get; set; }

        [Browsable(false)]
        [XmlElement("Kernel")]
        public string KernelXml
        {
            get { return ArrayConvert.ToString(Kernel); }
            set { Kernel = (float[])ArrayConvert.ToArray(value, 1, typeof(float)); }
        }

        public override CvMat Process(CvMat input)
        {
            if (Kernel != currentKernel)
            {
                UnloadKernel();
                currentKernel = Kernel;
                if (currentKernel != null && currentKernel.Length > 0)
                {
                    kernel = new CvMat(1, currentKernel.Length, CvMatDepth.CV_32F, 1);
                    Marshal.Copy(currentKernel, 0, kernel.Data, currentKernel.Length);

                    var anchor = Anchor;
                    if (anchor == -1) anchor = kernel.Cols / 2;
                    overlap = new CvMat(input.Rows, input.Cols + kernel.Cols - 1, input.Depth, input.NumChannels);
                    overlapInput = overlap.GetSubRect(new CvRect(kernel.Cols - 1, 0, input.Cols, input.Rows));
                    overlapEnd = overlap.GetSubRect(new CvRect(overlap.Cols - kernel.Cols + 1, 0, kernel.Cols - 1, input.Rows));
                    overlapStart = overlap.GetSubRect(new CvRect(0, 0, kernel.Cols - 1, input.Rows));
                    overlapOutput = new CvRect(kernel.Cols - anchor - 1, 0, input.Cols, input.Rows);
                    overlap.SetZero();
                }
            }

            if (kernel == null) return input;
            else
            {
                var output = new CvMat(overlap.Rows, overlap.Cols, overlap.Depth, overlap.NumChannels);
                Core.cvCopy(input, overlapInput);
                ImgProc.cvFilter2D(overlap, output, kernel, new CvPoint(Anchor, -1));
                Core.cvCopy(overlapEnd, overlapStart);
                return output.GetSubRect(overlapOutput);
            }
        }

        void UnloadKernel()
        {
            if (kernel != null)
            {
                kernel.Dispose();
                kernel = null;
                currentKernel = null;

                overlapInput.Close();
                overlapEnd.Close();
                overlapStart.Close();
                overlap.Close();
                overlap = overlapInput = null;
                overlapEnd = overlapStart = null;
            }
        }

        protected override void Unload()
        {
            UnloadKernel();
            base.Unload();
        }
    }
}

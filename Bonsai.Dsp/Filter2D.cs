using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Dsp
{
    public abstract class Filter2D<TArray> : Transform<TArray, TArray> where TArray : CvArr
    {
        CvMat kernel;
        float[,] currentKernel;

        public Filter2D()
        {
            Anchor = new CvPoint(-1, -1);
        }

        public CvPoint Anchor { get; set; }

        [XmlIgnore]
        [TypeConverter(typeof(MultidimensionalArrayConverter))]
        public float[,] Kernel { get; set; }

        [Browsable(false)]
        [XmlElement("Kernel")]
        public string KernelXml
        {
            get { return ArrayConvert.ToString(Kernel); }
            set { Kernel = ArrayConvert.ToArray<float>(value); }
        }

        protected abstract TArray CreateOutput(TArray input);

        public override TArray Process(TArray input)
        {
            if (Kernel != currentKernel)
            {
                currentKernel = Kernel;
                UnloadKernel();
                if (currentKernel != null && currentKernel.Length > 0)
                {
                    var rows = currentKernel.GetLength(0);
                    var columns = currentKernel.GetLength(1);
                    var kernelHandle = GCHandle.Alloc(currentKernel, GCHandleType.Pinned);
                    try
                    {
                        using (var kernelHeader = new CvMat(rows, columns, CvMatDepth.CV_32F, 1, kernelHandle.AddrOfPinnedObject()))
                        {
                            kernel = kernelHeader.Clone();
                        }

                    }
                    finally { kernelHandle.Free(); }
                }
            }

            if (kernel == null) return input;
            else
            {
                var output = CreateOutput(input);
                ImgProc.cvFilter2D(input, output, kernel, Anchor);
                return output;
            }
        }

        void UnloadKernel()
        {
            if (kernel != null)
            {
                kernel.Dispose();
                kernel = null;
                currentKernel = null;
            }
        }

        protected override void Unload()
        {
            UnloadKernel();
            base.Unload();
        }
    }

    public class Filter2D : Filter2D<CvMat>
    {
        protected override CvMat CreateOutput(CvMat input)
        {
            return new CvMat(input.Rows, input.Cols, input.Depth, input.NumChannels);
        }
    }
}

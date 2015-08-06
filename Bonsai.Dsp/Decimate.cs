using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Dsp
{
    public class Decimate : Combinator<Mat, Mat>
    {
        int factor;
        FrequencyFilter filter = new FrequencyFilter();

        public Decimate()
        {
            Factor = 1;
        }

        public int BufferLength { get; set; }

        public int Factor
        {
            get { return factor; }
            set
            {
                factor = value;
                UpdateCutoffFrequency(SamplingFrequency, value);
            }
        }

        public double SamplingFrequency
        {
            get { return filter.SamplingFrequency; }
            set
            {
                filter.SamplingFrequency = value;
                UpdateCutoffFrequency(value, Factor);
            }
        }

        public int KernelLength
        {
            get { return filter.KernelLength; }
            set { filter.KernelLength = value; }
        }

        void UpdateCutoffFrequency(double samplingFrequency, int factor)
        {
            if (factor > 0)
            {
                filter.Cutoff1 = samplingFrequency / (2 * factor);
            }
            else filter.Cutoff1 = 0;
        }

        static Mat CreateBuffer(int cols, Mat input)
        {
            return new Mat(input.Rows, cols, input.Depth, input.Channels);
        }

        public override IObservable<Mat> Process(IObservable<Mat> source)
        {
            return Observable.Create<Mat>(observer =>
            {
                var index = 0;
                var offset = 0;
                var currentFactor = 0;
                var buffer = default(Mat);
                return filter.Process(source).Subscribe(input =>
                {
                    try
                    {
                        var bufferLength = BufferLength;
                        if (bufferLength == 0) bufferLength = input.Cols;
                        if (buffer == null || buffer.Rows != input.Rows || currentFactor != factor)
                        {
                            index = 0;
                            offset = 0;
                            currentFactor = factor;
                            buffer = CreateBuffer(bufferLength, input);
                        }

                        while (offset < input.Cols)
                        {
                            // Process decimation data on this buffer
                            var samples = input.Cols - offset;
                            var whole = samples / currentFactor;
                            var outputRect = new Rect(index, 0, Math.Min(buffer.Cols - index, whole), input.Rows);
                            if (outputRect.Width > 1)
                            {
                                var inputRect = new Rect(offset, 0, outputRect.Width * currentFactor, input.Rows);
                                using (var inputBuffer = input.GetSubRect(inputRect))
                                using (var outputBuffer = buffer.GetSubRect(outputRect))
                                {
                                    CV.Resize(inputBuffer, outputBuffer, SubPixelInterpolation.NearestNeighbor);
                                }

                                index += outputRect.Width;
                                offset += inputRect.Width;
                            }
                            else
                            {
                                using (var inputBuffer = input.GetCol(offset))
                                using (var outputBuffer = buffer.GetCol(index))
                                {
                                    CV.Copy(inputBuffer, outputBuffer);
                                }

                                index++;
                                offset += currentFactor;
                            }

                            if (index >= buffer.Cols)
                            {
                                index = 0;
                                observer.OnNext(buffer);
                                buffer = CreateBuffer(bufferLength, input);
                            }
                        }

                        offset -= input.Cols;
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }
                },
                observer.OnError,
                () =>
                {
                    // Emit pending buffer
                    if (buffer != null)
                    {
                        observer.OnNext(buffer.GetCols(0, index));
                        buffer = null;
                    }

                    observer.OnCompleted();
                });
            });
        }
    }
}

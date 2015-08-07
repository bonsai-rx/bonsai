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
            Downsampling = DownsamplingMethod.LowPass;
        }

        public DownsamplingMethod Downsampling { get; set; }

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
                var lottery = 0;
                var currentFactor = 0;
                var buffer = default(Mat);
                var downsampling = Downsampling;
                var random = downsampling == DownsamplingMethod.Dithering ? new Random() : null;
                var downsample = downsampling == DownsamplingMethod.LowPass ? filter.Process(source) : source;
                return downsample.Subscribe(input =>
                {
                    try
                    {
                        var bufferLength = BufferLength;
                        if (bufferLength == 0) bufferLength = input.Cols;
                        if (buffer == null || buffer.Rows != input.Rows || currentFactor != factor)
                        {
                            index = 0;
                            currentFactor = factor;
                            if (random != null)
                            {
                                lottery = random.Next(currentFactor);
                                offset = lottery;
                            }
                            else offset = 0;
                            buffer = CreateBuffer(bufferLength, input);
                        }

                        while (offset < input.Cols)
                        {
                            // Process decimation data on this buffer
                            Rect outputRect;
                            if (random != null)
                            {
                                outputRect = new Rect(index, 0, 1, input.Rows);
                            }
                            else
                            {
                                var samples = input.Cols - offset;
                                var whole = samples / currentFactor;
                                outputRect = new Rect(index, 0, Math.Min(buffer.Cols - index, whole), input.Rows);
                            }

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
                                if (random != null)
                                {
                                    offset += currentFactor - lottery;
                                    lottery = random.Next(currentFactor);
                                    offset += lottery;
                                }
                                else offset += currentFactor;
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

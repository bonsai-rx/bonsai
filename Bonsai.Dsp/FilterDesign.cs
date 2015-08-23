using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Dsp
{
    static class FilterDesign
    {
        internal struct PoleZero
        {
            internal Complex[] Poles;
            internal Complex[] Zeros;
            internal double Gain;
        }

        internal static PoleZero ButterworthPrototype(int n)
        {
            // Specify nth-order butterworth filter poles using
            // sk = e^[j(2k + n - 1)pi / 2n]
            // http://en.wikipedia.org/wiki/Butterworth_filter
            var poles = new Complex[n];
            for (int i = 0; i < poles.Length; i++)
            {
                var k = i + 1;
                poles[i] = Complex.Exp(Complex.ImaginaryOne * (2 * k + n - 1) * Math.PI / (2.0 * n));
            }

            PoleZero prototype;
            prototype.Poles = poles;
            prototype.Zeros = new Complex[0];
            prototype.Gain = 1;
            return prototype;
        }

        static Complex[] Coefficients(Complex[] zeros)
        {
            // Expand polynomial coefficients by incremental convolution
            var result = new Complex[zeros.Length + 1];
            result[0] = 1;
            for (int i = 0; i < zeros.Length; i++)
            {
                var previous = result[0];
                for (int j = 1; j <= i + 1; j++)
                {
                    var z = previous * -zeros[i];
                    previous = result[j];
                    result[j] += z;
                }
            }
            return result;
        }

        static void TransferFunction(PoleZero filter, out Complex[] b, out Complex[] a)
        {
            // Conversion from pole-zero specification to transfer function
            b = Coefficients(filter.Zeros);
            a = Coefficients(filter.Poles);
            for (int i = 0; i < b.Length; i++)
            {
                b[i] *= filter.Gain;
            }
        }

        static PoleZero Reduce(PoleZero filter)
        {
            var poles = new List<Complex>();
            var zeros = new List<Complex>();
            var hitMap = new Dictionary<Complex, int>();
            for (int i = 0; i < filter.Poles.Length; i++)
            {
                int hits;
                var pole = filter.Poles[i];
                hitMap.TryGetValue(pole, out hits);
                hitMap[pole] = hits + 1;
                poles.Add(pole);
            }

            for (int i = 0; i < filter.Zeros.Length; i++)
            {
                int hits;
                var zero = filter.Zeros[i];
                hitMap.TryGetValue(zero, out hits);
                if (hits > 0)
                {
                    hitMap[zero]--;
                    poles.Remove(zero);
                }
                else zeros.Add(zero);
            }

            PoleZero result;
            result.Poles = poles.ToArray();
            result.Zeros = zeros.ToArray();
            result.Gain = filter.Gain;
            return result;
        }

        static PoleZero LowPass(PoleZero filter, double fc, double c)
        {
            // Apply lowpass to lowpass frequency transformation:
            // http://en.wikipedia.org/wiki/Prototype_filter
            var filterPoles = filter.Poles;
            var filterZeros = filter.Zeros;
            var poles = new List<Complex>();
            var zeros = new List<Complex>();
            var gain = Complex.One;

            for (int i = 0; i < filterPoles.Length; i++)
            {
                poles.Add(fc * filterPoles[i] / c);
                gain *= fc / c;
            }

            for (int i = 0; i < filterZeros.Length; i++)
            {
                zeros.Add(fc * filterZeros[i] / c);
                gain *= c / fc;
            }

            PoleZero lowPass;
            lowPass.Poles = poles.ToArray();
            lowPass.Zeros = zeros.ToArray();
            lowPass.Gain = (filter.Gain * gain).Real;
            return lowPass;
        }

        static PoleZero HighPass(PoleZero filter, double fc, double c)
        {
            // Apply lowpass to highpass frequency transformation:
            // http://en.wikipedia.org/wiki/Prototype_filter
            var filterPoles = filter.Poles;
            var filterZeros = filter.Zeros;
            var poles = new List<Complex>();
            var zeros = new List<Complex>();
            var gain = Complex.One;

            for (int i = 0; i < filterPoles.Length; i++)
            {
                zeros.Add(0);
                poles.Add(fc * c / filterPoles[i]);
                gain *= -1 / filterPoles[i];
            }

            for (int i = 0; i < filterZeros.Length; i++)
            {
                poles.Add(0);
                zeros.Add(fc * c / filterZeros[i]);
                gain *= -filterPoles[i];
            }

            PoleZero highPass;
            highPass.Poles = poles.ToArray();
            highPass.Zeros = zeros.ToArray();
            highPass.Gain = (filter.Gain * gain).Real;
            return highPass;
        }

        static PoleZero BandPass(PoleZero filter, double fl, double fh, double c)
        {
            // Apply lowpass to bandpass frequency transformation:
            // http://en.wikipedia.org/wiki/Prototype_filter
            var filterPoles = filter.Poles;
            var filterZeros = filter.Zeros;
            var poles = new List<Complex>();
            var zeros = new List<Complex>();
            var gain = Complex.One;

            for (int i = 0; i < filterPoles.Length; i++)
            {
                var b = (filterPoles[i] / c) * (fh - fl) / 2;
                zeros.Add(0);
                poles.Add(b + Complex.Sqrt(b * b - fh * fl));
                poles.Add(b - Complex.Sqrt(b * b - fh * fl));
                gain *= (fh - fl) / c;
            }

            for (int i = 0; i < filterZeros.Length; i++)
            {
                var b = (filterZeros[i] / c) * (fh - fl) / 2;
                poles.Add(0);
                zeros.Add(b + Complex.Sqrt(b * b - fh * fl));
                zeros.Add(b - Complex.Sqrt(b * b - fh * fl));
                gain *= c / (fh - fl);
            }

            PoleZero bandPass;
            bandPass.Poles = poles.ToArray();
            bandPass.Zeros = zeros.ToArray();
            bandPass.Gain = (filter.Gain * gain).Real;
            return bandPass;
        }

        static PoleZero BandStop(PoleZero filter, double fl, double fh, double c)
        {
            // Apply lowpass to lowpass frequency transformation:
            // http://en.wikipedia.org/wiki/Prototype_filter
            var filterPoles = filter.Poles;
            var filterZeros = filter.Zeros;
            var poles = new List<Complex>();
            var zeros = new List<Complex>();
            var gain = Complex.One;

            for (int i = 0; i < filterPoles.Length; i++)
            {
                var b = (c / filterPoles[i]) * (fh - fl) / 2;
                zeros.Add(Complex.Sqrt(-fh * fl));
                zeros.Add(-Complex.Sqrt(-fh * fl));
                poles.Add(b + Complex.Sqrt(b * b - fh * fl));
                poles.Add(b - Complex.Sqrt(b * b - fh * fl));
                gain *= -1 / filterPoles[i];
            }

            for (int i = 0; i < filterZeros.Length; i++)
            {
                var b = (filterZeros[i] / c) * (fh - fl) / 2;
                poles.Add(Complex.Sqrt(-fh * fl));
                poles.Add(-Complex.Sqrt(-fh * fl));
                zeros.Add(b + Complex.Sqrt(b * b - fh * fl));
                zeros.Add(b - Complex.Sqrt(b * b - fh * fl));
                gain *= -filterPoles[i];
            }

            PoleZero bandStop;
            bandStop.Poles = poles.ToArray();
            bandStop.Zeros = zeros.ToArray();
            bandStop.Gain = (filter.Gain * gain).Real;
            return bandStop;
        }

        static PoleZero BilinearTransform(PoleZero sFilter, double fs)
        {
            // Convert from s-domain to z-domain using the bilinear transform:
            // s = K * (z-1) / (z+1); K = 2 / fs
            // http://en.wikipedia.org/wiki/Bilinear_transform
            var sPoles = sFilter.Poles;
            var sZeros = sFilter.Zeros;
            var poles = new List<Complex>();
            var zeros = new List<Complex>();
            var gain = Complex.One;

            for (int i = 0; i < sPoles.Length; i++)
            {
                zeros.Add(-1);
                poles.Add((2 + sPoles[i] * fs) / (2 - sPoles[i] * fs));
                gain *= fs / (2 - sPoles[i] * fs);
            }

            for (int i = 0; i < sZeros.Length; i++)
            {
                poles.Add(-1);
                zeros.Add((2 + sZeros[i] * fs) / (2 - sZeros[i] * fs));
                gain *= (2 - sZeros[i] * fs) / fs;
            }

            PoleZero zFilter;
            zFilter.Poles = poles.ToArray();
            zFilter.Zeros = zeros.ToArray();
            zFilter.Gain = (sFilter.Gain * gain).Real;
            return zFilter;
        }

        internal static void GetFilterCoefficients(PoleZero sFilter, double[] fs, FilterType ftype, out double[] b, out double[] a)
        {
            // Pre-warp cutoff frequency
            fs = Array.ConvertAll(fs, fc => 2 * Math.Tan(Math.PI * fc));

            // Transform filter frequencies
            switch (ftype)
            {
                case FilterType.LowPass:
                    sFilter = LowPass(sFilter, fs[0], 1.0);
                    break;
                case FilterType.HighPass:
                    sFilter = HighPass(sFilter, fs[0], 1.0);
                    break;
                case FilterType.BandPass:
                    sFilter = BandPass(sFilter, fs[0], fs[1], 1.0);
                    break;
                case FilterType.BandStop:
                    sFilter = BandStop(sFilter, fs[0], fs[1], 1.0);
                    break;
            }

            // Convert from s-domain to z-domain
            var zFilter = BilinearTransform(sFilter, 1.0);
            zFilter = Reduce(zFilter);

            // Output transfer function coefficients
            Complex[] bz, az;
            TransferFunction(zFilter, out bz, out az);
            b = Array.ConvertAll(bz, c => c.Real);
            a = Array.ConvertAll(az, c => c.Real);
        }
    }
}

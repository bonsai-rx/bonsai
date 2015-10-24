using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZedGraph;

namespace Bonsai.Dsp.Design
{
    class DownsampledPointPairList : IPointList
    {
        double skip;
        double? minBounds;
        double? maxBounds;
        int minIndex = -1;
        int maxIndex = -1;
        PointPairList list;
        int addIndex;
        Random random;
        int[] decimation;
        int historyLength;

        public DownsampledPointPairList()
            : this(null, new Random())
        {
        }

        public DownsampledPointPairList(Random random)
            : this(null, random)
        {
        }

        public DownsampledPointPairList(PointPairList list, Random random)
        {
            this.list = list != null ? list.Clone() : new PointPairList();
            this.random = random;
        }

        public int HistoryLength
        {
            get { return historyLength; }
            set
            {
                var dither = historyLength != value;
                historyLength = value;
                UpdateDecimation(dither);
            }
        }

        public void SetBounds(double min, double max, int maxPoints)
        {
            var dither = minBounds != min || maxBounds != max;
            minBounds = min;
            maxBounds = max;
            if (decimation == null || decimation.Length != maxPoints)
            {
                decimation = new int[maxPoints];
            }
            UpdateDecimation(dither);
        }

        private void UpdateDecimation(bool dither)
        {
            minIndex = minBounds.HasValue ? Math.Max(0, Math.Min((int)minBounds, HistoryLength - 1)) : 0;
            maxIndex = maxBounds.HasValue ? Math.Max(1, Math.Min((int)Math.Ceiling(maxBounds.Value) + 1, HistoryLength)) : HistoryLength;

            var pointCount = maxIndex - minIndex;
            if (decimation == null || pointCount <= decimation.Length) skip = 1;
            else
            {
                skip = pointCount / (double)decimation.Length;
                if (dither)
                {
                    for (int i = 0; i < decimation.Length; i++)
                    {
                        int start = (int)(i * skip + minIndex);
                        int next = Math.Min(HistoryLength, (int)((i + 1) * skip + minIndex));
                        decimation[i] = random.Next(start, next);
                    }
                }
            }
        }

        public int Count
        {
            get
            {
                if (!minBounds.HasValue) return list.Count;
                return Math.Min(decimation.Length, maxIndex - minIndex);
            }
        }

        public object Clone()
        {
            return new DownsampledPointPairList(list, random);
        }

        public void Add(double y)
        {
            var excess = list.Count - HistoryLength;
            if (excess > 0)
            {
                list.RemoveRange(list.Count - excess, excess);
                addIndex %= HistoryLength;
            }

            if (list.Count < HistoryLength && addIndex == list.Count) list.Add(addIndex, y);
            else list[addIndex].Y = y;

            // We want to update the dithering (random decimation) only if the current index
            // is the start of a sample block. This appears to be true iff the condition:
            // (addIndex - minIndex) % skip > 1
            var sample = addIndex - minIndex;
            if (skip > 1 && sample >= 0 && addIndex < maxIndex && sample % skip <= 1)
            {
                // The index of the decimation to update is computed by solving the decimation
                // formula for the index, using ceil instead of floor (truncation):
                // i = ceil((addIndex - minIndex) / skip)
                var decimated = (int)Math.Ceiling(sample / skip);
                if (decimated < decimation.Length)
                {
                    int start = (int)(decimated * skip + minIndex);
                    int next = Math.Min(HistoryLength, (int)((decimated + 1) * skip + minIndex));
                    decimation[decimated] = random.Next(start, next);
                }
            }

            addIndex = (addIndex + 1) % HistoryLength;
        }

        public void Clear()
        {
            list.Clear();
        }

        public PointPair this[int index]
        {
            get
            {
                if (!minBounds.HasValue) return list[index];
                if (skip == 1)
                {
                    index += minIndex;
                }
                else index = decimation[index];

                if (index < list.Count) return list[index];
                else return new PointPair(PointPair.Missing, PointPair.Missing);
            }
        }
    }
}

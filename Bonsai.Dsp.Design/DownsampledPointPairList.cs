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
        int maxPoints;
        int minIndex = -1;
        int maxIndex = -1;
        PointPairList list;
        int addIndex;

        public DownsampledPointPairList()
            : this(new PointPairList())
        {
        }

        public DownsampledPointPairList(PointPairList list)
        {
            this.list = list;
        }

        public int HistoryLength { get; set; }

        public void SetBounds(double min, double max, int maxPoints)
        {
            minIndex = list.BinarySearch(new PointPair(min, 0), PointComparer.Default);
            maxIndex = list.BinarySearch(new PointPair(max, 0), PointComparer.Default);

            if (minIndex < 0) minIndex = Math.Max(0, Math.Min(~minIndex, list.Count - 1)) - 1;
            if (maxIndex < 0) maxIndex = Math.Max(0, Math.Min(~maxIndex, list.Count - 1)) - 1;
            var pointCount = maxIndex - minIndex;
            skip = pointCount < maxPoints ? 1 : pointCount / (double)maxPoints;
            this.maxPoints = maxPoints;
        }

        public PointPairList List
        {
            get { return list; }
        }

        public int Count
        {
            get
            {
                if (minIndex < 0) return list.Count;
                return Math.Min(maxPoints, maxIndex - minIndex);
            }
        }

        public object Clone()
        {
            return new DownsampledPointPairList(list.Clone());
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
                if (minIndex < 0) return list[index];
                if (Count < maxPoints) return list[index + minIndex];

                int start = (int)(index * skip + minIndex);
                int next = (int)((index + 1) * skip + minIndex);
                PointPair max = new PointPair(list[start]);
                for (int i = start + 1; i < next; i++)
                {
                    max.Y = Math.Max(max.Y, list[i].Y);
                }

                return max;
            }
        }

        class PointComparer : IComparer<PointPair>
        {
            internal static readonly PointComparer Default = new PointComparer();

            public int Compare(PointPair x, PointPair y)
            {
                var xdiff = x.X - y.X;
                return xdiff < 0 ? -1 : xdiff > 0 ? 1 : 0;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Vision
{
    public class KeyPointOpticalFlow
    {
        public KeyPointOpticalFlow(KeyPointCollection previousKeyPoints, KeyPointCollection currentKeyPoints)
        {
            if (previousKeyPoints == null) throw new ArgumentNullException("previousKeyPoints");
            if (currentKeyPoints == null) throw new ArgumentNullException("currentKeyPoints");

            PreviousKeyPoints = previousKeyPoints;
            CurrentKeyPoints = currentKeyPoints;
        }

        public KeyPointCollection PreviousKeyPoints { get; private set; }

        public KeyPointCollection CurrentKeyPoints { get; private set; }
    }
}

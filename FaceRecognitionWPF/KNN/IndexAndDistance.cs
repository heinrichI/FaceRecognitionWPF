using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionWPF.KNN
{
    public class IndexAndDistance : IComparable<IndexAndDistance>
    {
        public int Index;  // index of a training item
        public double Distance;  // distance from train item to unknown

        // need to sort these to find k closest
        public int CompareTo(IndexAndDistance other)
        {
            if (this.Distance < other.Distance) return -1;
            else if (this.Distance > other.Distance) return +1;
            else return 0;
        }
    }
}

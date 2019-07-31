using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionWPF.KNN
{
    public class IndexAndDistance : IComparable<IndexAndDistance>
    {
        public int idx;  // index of a training item
        public double dist;  // distance from train item to unknown

        // need to sort these to find k closest
        public int CompareTo(IndexAndDistance other)
        {
            if (this.dist < other.dist) return -1;
            else if (this.dist > other.dist) return +1;
            else return 0;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionWPF.KNN
{
    class VoteAndDistance
    {
        public VoteAndDistance(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public int Vote { get; internal set; }

        public double Distance { get; internal set; }
    }
}

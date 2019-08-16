using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionBusinessLogic.KNN
{
    public class VoteAndDistance
    {
        public VoteAndDistance(string name)
        {
            Name = name;
            SortedInfos = new List<ClassInfo>();
        }

        public string Name { get; }

        public int Vote { get; internal set; }

        public double Distance { get; internal set; }

        public List<ClassInfo> SortedInfos { get; internal set; }

        public double[] TestData { get; internal set; }

        public override string ToString()
        {
            return $"{Name} - {Distance} - {Vote}";
        }
    }
}

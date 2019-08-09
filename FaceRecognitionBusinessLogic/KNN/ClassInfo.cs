using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionBusinessLogic.KNN
{
    public class ClassInfo
    {
        public ClassInfo(string className, double[] data)
        {
            Name = className;
            Data = data;
            //Data = new List<double>();
            //Encodings = new List<FaceEncoding>();
        }

        public string Name { get; }

        //public List<double> Data { get; }

        public double[] Data { get; }

        //public List<FaceEncoding> Encodings { get; }
    }
}

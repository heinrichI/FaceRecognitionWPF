using FaceRecognitionBusinessLogic.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionBusinessLogic.KNN
{
    public class ClassInfo
    {
        public ClassInfo(string className, double[] data, FaceLocation faceLocation)
        {
            Name = className;
            Data = data;
            //Data = new List<double>();
            //Encodings = new List<FaceEncoding>();
            FaceLocation = faceLocation;
        }

        public string Name { get; }

        //public List<double> Data { get; }

        public double[] Data { get; }

        //public List<FaceEncoding> Encodings { get; }

        public FaceLocation FaceLocation { get; }
    }
}

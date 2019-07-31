using FaceRecognitionDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionWPF.KNN
{
    class ClassInfo
    {
        public ClassInfo(string directory)
        {
            Name = directory;
            Data = new List<double>();
            //Encodings = new List<FaceEncoding>();
        }

        public string Name { get; }

        public List<double> Data { get; }

        //public List<FaceEncoding> Encodings { get; }
    }
}

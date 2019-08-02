using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionBusinessLogic.DataBase
{
    public class FaceEncodingInfo
    {
        public FaceEncodingInfo()
        {
            FingerPrints = new List<double[]>();
        }

        public FaceEncodingInfo(string path) : this()
        {
            this.Path = path;
        }

        //[BsonId]
        public string Path { get; set; }

        public List<double[]> FingerPrints { get; set; }
    }
}

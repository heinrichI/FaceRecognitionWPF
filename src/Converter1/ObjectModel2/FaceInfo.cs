using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Converter1.ObjectModel2
{
    public class FaceInfo
    {
        public FaceInfo()
        {
        }

        public FaceInfo(string md5) 
        {
            this.Md5 = md5;
            FingerAndLocations = new List<FingerAndLocation>();
        }

        public string Md5 { get; set; }

        public List<FingerAndLocation> FingerAndLocations { get; set; }
    }
}

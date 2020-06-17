using Converter1.ObjectModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Converter1.ObjectModel1
{
    public class FaceEncodingInfo
    {
        public FaceEncodingInfo()
        {
            FingerAndLocations = new List<FingerAndLocation>();
            //NotPerson = new List<string>();
        }

        public FaceEncodingInfo(string path) : this()
        {
            this.Path = path;

            var fi = new FileInfo(path);
            Length = fi.Length;
            LastWriteTime = fi.LastWriteTime;
        }

        //[BsonId]
        public string Path { get; set; }

        public long Length { get; set; }

        public DateTime LastWriteTime { get; set; }

        public List<FingerAndLocation> FingerAndLocations { get; set; }

        //public List<string> NotPerson { get; set; }
    }
}

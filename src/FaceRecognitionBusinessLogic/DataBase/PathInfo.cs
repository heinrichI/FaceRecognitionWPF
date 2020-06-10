using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionBusinessLogic.DataBase
{
    public class PathInfo
    {
        public PathInfo(string path)
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


        //public List<string> NotPerson { get; set; }
    }
}

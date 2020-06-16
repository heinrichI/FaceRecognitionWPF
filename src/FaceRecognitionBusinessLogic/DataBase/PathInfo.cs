using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionBusinessLogic.DataBase
{
    public class PathInfo
    {
        public PathInfo(string path, string md5, long length, DateTime lastWriteTime)
        {
            this.Path = path;

            Length = length;
            LastWriteTime = lastWriteTime;
            Md5 = md5;
        }

        //[BsonId]
        public string Path { get; set; }

        public long Length { get; set; }

        public DateTime LastWriteTime { get; set; }

        public string Md5 { get; set; }

        //public List<string> NotPerson { get; set; }
    }
}

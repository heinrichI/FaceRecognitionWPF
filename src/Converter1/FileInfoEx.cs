using Converter1.ObjectModel;
using Converter1.ObjectModel1;
using System;
using System.Collections.Generic;
using System.IO;

namespace Converter1
{
    internal class FileInfoEx
    {
        public FileInfoEx(FaceEncodingInfo f)
        {
            this.FingerAndLocations = f.FingerAndLocations;

            Path = f.Path.ToLower();
            Md5 = HashHelper.CreateMD5Checksum(Path);

            FileInfo fi = new FileInfo(Path);
            Length = fi.Length;
            LastWriteTime = fi.LastWriteTime;
        }

        public List<FingerAndLocation> FingerAndLocations { get; }
        public string Path { get; }
        public string Md5 { get; }
        public long Length { get; }
        public DateTime LastWriteTime { get; }
    }
}
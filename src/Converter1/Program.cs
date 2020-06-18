using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Converter1
{
    class Program
    {
        static void Main(string[] args)
        {
            DataBaseManager dbOld = new DataBaseManager("faces.litedb");
            DataBaseManager2 dbNew = new DataBaseManager2("path.litedb", "md5.litedb");
            var infos = dbOld.GetAll().ToList();

            int count = infos.Count;
            //Console.WriteLine($"Total {count}");
            int i = 1;
            //foreach (ObjectModel1.FaceEncodingInfo item in infos)
            //{
            //    if (!File.Exists(item.Path))
            //        continue;

            //    if (item.FingerAndLocations.Any())
            //    {
            //        foreach (var face in item.FingerAndLocations)
            //        {
            //            dbNew.AddFaceInfo(item.Path, face.FingerPrint, face.Left, face.Right, face.Top, face.Bottom);
            //        }
            //    }
            //    else
            //    {
            //        dbNew.AddFileWithoutFace(item.Path);
            //    }
            //    Console.WriteLine($"{i++}/{count}");
            //}

            List<ObjectModel1.FaceEncodingInfo> face = new List<ObjectModel1.FaceEncodingInfo>();
            foreach (ObjectModel1.FaceEncodingInfo item in infos)
            {
                if (!File.Exists(item.Path))
                    continue;

                face.Add(item);
                Console.WriteLine($"{i++}/{count}");
            }

            //var faceFingerAndLocations = face
            //    .SelectMany(f => f.FingerAndLocations, (parent, child) =>
            //    new { parent.Path, child.FingerPrint, child.Left, child.Right, child.Top, child.Bottom });

            Console.WriteLine("Compute md5");
            var faceFingerAndLocations = face
                .GroupBy(f => f.Path)
                .Select(g => new FileInfoEx(g.First())).ToList();

            //var group = faceFingerAndLocations.GroupBy(f => f.Md5);
            //var duplices = group.Where(x => x.Count() > 1);
            //Console.WriteLine($"Duplicate: {duplices.Count()}");

            Console.WriteLine("Write to db");
            dbNew.AddFacesInfo(faceFingerAndLocations);
        }
    }
}

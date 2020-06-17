using System;
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
            var infos = dbOld.GetAll();

            int count = infos.Count();
            Console.WriteLine($"Total {count}");
            int i = 1;
            foreach (var item in infos)
            {
                if (!File.Exists(item.Path))
                    continue;

                if (item.FingerAndLocations.Any())
                {
                    foreach (var face in item.FingerAndLocations)
                    {
                        dbNew.AddFaceInfo(item.Path, face.FingerPrint, face.Left, face.Right, face.Top, face.Bottom);
                    }
                }
                else
                {
                    dbNew.AddFileWithoutFace(item.Path);
                }
                Console.WriteLine($"{i++}/{count}");
            }
        }
    }
}

using FaceRecognitionBusinessLogic;
using FaceRecognitionBusinessLogic.DataBase;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionDataBase
{
    public class DataBaseManager : IDataBaseManager
    {
        LiteDatabase _pathDb;
        LiteDatabase _md5Db;

        LiteCollection<PathInfo> _pathCollection;
        LiteCollection<FingerAndLocation> _fingerCollection;
        LiteCollection<FaceInfo> _md5Collection;

        public DataBaseManager(string pathDbName, string md5DbName)
        {
            ////// Re-use mapper from global instance
            var mapper = BsonMapper.Global;


            //mapper.Entity<FingerAndLocation>()
            //    .Id(x => x.Id);

            //// "Products" and "Customer" are from other collections (not embedded document)
            mapper.Entity<FaceInfo>()
                .DbRef(x => x.FingerAndLocations, "FingerAndLocations")    // 1 to Many reference
                 //.DbRef(x => x.NotPerson, "NotPerson")
                .Id(f => f.Md5);

            _pathDb = new LiteDatabase(pathDbName);

            _pathCollection = _pathDb.GetCollection<PathInfo>("FaceEncodingInfo");
            _pathCollection.EnsureIndex(x => x.Path);

            _fingerCollection = _pathDb.GetCollection<FingerAndLocation>("FingerAndLocations");

            _md5Db = new LiteDatabase(md5DbName);
            _md5Collection = _md5Db.GetCollection<FaceInfo>("FaceInfo");
            _md5Collection.EnsureIndex(x => x.Md5);
        }

        public void Dispose()
        {
            _pathDb.Dispose();
            _md5Db.Dispose();
        }

        public FaceInfo GetFromDB(string imageFilePath)
        {
            try
            {
                string imageFileLower = imageFilePath.ToLower();
                var pathInfo = _pathCollection.FindById(imageFileLower);
                if (pathInfo != null)
                {
                    var fi = new FileInfo(imageFileLower);
                    //Debug.WriteLine(fi.LastWriteTime.Ticks);
                    if (fi.LastWriteTime.ToLongTimeString() == pathInfo.LastWriteTime.ToLongTimeString()
                        && fi.LastWriteTime.ToLongDateString() == pathInfo.LastWriteTime.ToLongDateString()
                        && fi.Length == pathInfo.Length)
                    {
                        var info = _md5Collection.Include(x => x.FingerAndLocations).FindById(pathInfo.Path);

                        return info;
                    }
                    else
                        Debug.WriteLine($"Не совпадает LastWriteTime Length {imageFilePath}");
                }
                else
                {
                    string md5 = HashHelper.CreateMD5Checksum(imageFilePath);

                    var info = _md5Collection.Include(x => x.FingerAndLocations).FindById(md5);

                    _pathCollection.Upsert(pathInfo);

                    return info;
                }
            }
            catch (InvalidCastException ex)
            {
                bool result = _pathCollection.Delete(imageFilePath);
                return null;
            }
            return null;
        }
        
        public void AddFaceInfo(string imageFile, double[] doubleInfo, int left, int right, int top, int bottom)
        {
            string imageFileLower = imageFile.ToLower();
            FaceInfo faceInfo = GetFromDB(imageFileLower);
            //if (faceEncodingInfo != null)
            //    throw new Exception($"{imageFile} уже есть в базе!");

            //if (pathInfo == null)
            //    pathInfo = new PathInfo(imageFileLower);

            FingerAndLocation fingerAndLocation = new FingerAndLocation();
            fingerAndLocation.FingerPrint = doubleInfo;
            fingerAndLocation.Left = left;
            fingerAndLocation.Right = right;
            fingerAndLocation.Top = top;
            fingerAndLocation.Bottom = bottom;

            var fingerAndLocations  = _fingerCollection.Find(f => f.Equals(fingerAndLocation));

            if (fingerAndLocations.Any())
                fingerAndLocation.Id = fingerAndLocations.Single().Id;
            else
            {
                _fingerCollection.Insert(fingerAndLocation);
            }

            //if (!faceEncodingInfo.FingerAndLocations.Any(fe => fe.Equals(fingerAndLocation)))
            if (!faceInfo.FingerAndLocations.Any(fe => fe.Bottom == fingerAndLocation.Bottom
            && fe.Left == fingerAndLocation.Left
            && fe.Right == fingerAndLocation.Right
            && fe.Top == fingerAndLocation.Top
            && fe.FingerPrint.SequenceEqual(fingerAndLocation.FingerPrint)))
                faceInfo.FingerAndLocations.Add(fingerAndLocation);

           // _pathCollection.Upsert(faceInfo);
            _md5Collection.Upsert(faceInfo);

            //var info = _faceCollection.Include(x => x.FingerAndLocations).FindById(imageFile);
            //if (info.FingerAndLocations.First().Left != faceEncodingInfo.FingerAndLocations.First().Left)
            //    throw new Exception("Данные не сохранились!");
        }

        public void AddFileWithoutFace(string imageFile)
        {
            string imageFileLower = imageFile.ToLower();

            FaceInfo faceEncodingInfo = GetFromDB(imageFileLower);
            if (faceEncodingInfo != null)
                throw new Exception($"{imageFile} уже есть в базе!");

            PathInfo pathInfo = new PathInfo(imageFileLower);
            _pathCollection.Upsert(pathInfo);

            faceEncodingInfo = new FaceInfo(faceEncodingInfo.Md5);
            _md5Collection.Upsert(faceEncodingInfo);
        }

        public IEnumerable<FaceInfo> GetAll()
        {
            return _md5Collection.Include(x => x.FingerAndLocations).FindAll();
        }

        public int Remove(string path)
        {
            return _pathCollection.Delete((PathInfo info) => info.Path == path);
        }

        public bool UpsertFaceInfo(PathInfo info)
        {
            return _pathCollection.Upsert(info);
        }

        public long Shrink()
        {
            return _pathDb.Engine.Shrink();
        }
    }
}

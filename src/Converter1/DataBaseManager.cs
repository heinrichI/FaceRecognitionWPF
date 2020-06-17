using Converter1.ObjectModel;
using Converter1.ObjectModel1;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Converter1
{
    public class DataBaseManager
    {
        LiteDatabase _db;
        LiteCollection<FaceEncodingInfo> _faceCollection;
        LiteCollection<FingerAndLocation> _fingerCollection;

        public DataBaseManager(string dataBaseName)
        {
            ////// Re-use mapper from global instance
            var mapper = BsonMapper.Global;


            //mapper.Entity<FingerAndLocation>()
            //    .Id(x => x.Id);

            //// "Products" and "Customer" are from other collections (not embedded document)
            mapper.Entity<FaceEncodingInfo>()
                .DbRef(x => x.FingerAndLocations, "FingerAndLocations")    // 1 to Many reference
                 //.DbRef(x => x.NotPerson, "NotPerson")
                .Id(f => f.Path);

            _db = new LiteDatabase(dataBaseName);
            //_db.Shrink();

            _faceCollection = _db.GetCollection<FaceEncodingInfo>("FaceEncodingInfo");
            _faceCollection.EnsureIndex(x => x.Path);

            _fingerCollection = _db.GetCollection<FingerAndLocation>("FingerAndLocations");
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        public FaceEncodingInfo GetFromDB(string imageFile)
        {
            try
            {
                string imageFileLower = imageFile.ToLower();
                var info = _faceCollection.Include(x => x.FingerAndLocations).FindById(imageFileLower);
                if (info != null)
                {
                    var fi = new FileInfo(imageFileLower);
                    //Debug.WriteLine(fi.LastWriteTime.Ticks);
                    if (fi.LastWriteTime.ToLongTimeString() == info.LastWriteTime.ToLongTimeString()
                        && fi.LastWriteTime.ToLongDateString() == info.LastWriteTime.ToLongDateString()
                        && fi.Length == info.Length)
                        return info;
                    else
                        Debug.WriteLine($"Не совпадает LastWriteTime Length {imageFile}");
                }
            }
            catch (InvalidCastException ex)
            {
                bool result = _faceCollection.Delete(imageFile);
                return null;
            }
            return null;
        }


        public void AddFaceInfo(string imageFile, double[] doubleInfo, int left, int right, int top, int bottom)
        {
            string imageFileLower = imageFile.ToLower();
            FaceEncodingInfo faceEncodingInfo = GetFromDB(imageFileLower);
            //if (faceEncodingInfo != null)
            //    throw new Exception($"{imageFile} уже есть в базе!");

            if (faceEncodingInfo == null)
                faceEncodingInfo = new FaceEncodingInfo(imageFileLower);

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
            if (!faceEncodingInfo.FingerAndLocations.Any(fe => fe.Bottom == fingerAndLocation.Bottom
            && fe.Left == fingerAndLocation.Left
            && fe.Right == fingerAndLocation.Right
            && fe.Top == fingerAndLocation.Top
            && fe.FingerPrint.SequenceEqual(fingerAndLocation.FingerPrint)))
                faceEncodingInfo.FingerAndLocations.Add(fingerAndLocation);

            _faceCollection.Upsert(faceEncodingInfo);

            //var info = _faceCollection.Include(x => x.FingerAndLocations).FindById(imageFile);
            //if (info.FingerAndLocations.First().Left != faceEncodingInfo.FingerAndLocations.First().Left)
            //    throw new Exception("Данные не сохранились!");
        }

        public void AddFileWithoutFace(string imageFile)
        {
            string imageFileLower = imageFile.ToLower();

            FaceEncodingInfo faceEncodingInfo = GetFromDB(imageFileLower);
            if (faceEncodingInfo != null)
                throw new Exception($"{imageFile} уже есть в базе!");

            faceEncodingInfo = new FaceEncodingInfo(imageFileLower);
            _faceCollection.Upsert(faceEncodingInfo);
        }

        public IEnumerable<FaceEncodingInfo> GetAll()
        {
            return _faceCollection.Include(x => x.FingerAndLocations).FindAll();
        }

        public int Remove(string path)
        {
            return _faceCollection.Delete((FaceEncodingInfo info) => info.Path == path);
        }

        public bool UpsertFaceInfo(FaceEncodingInfo info)
        {
            return _faceCollection.Upsert(info);
        }

        public long Shrink()
        {
            return _db.Engine.Shrink();
        }
    }
}

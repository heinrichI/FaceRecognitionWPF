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
            var info = _faceCollection.Include(x => x.FingerAndLocations).FindById(imageFile);
            if (info != null)
            {
                var fi = new FileInfo(imageFile);
                //Debug.WriteLine(fi.LastWriteTime.Ticks);
                if (fi.LastWriteTime.ToLongTimeString() == info.LastWriteTime.ToLongTimeString()
                    && fi.LastWriteTime.ToLongDateString() == info.LastWriteTime.ToLongDateString()
                    && fi.Length == info.Length)
                    return info;
                else
                    Debug.WriteLine($"Не совпадает LastWriteTime Length {imageFile}");
            }
            return null;
        }


        public void AddFaceInfo(string imageFile, double[] doubleInfo, int left, int right, int top, int bottom)
        {
            FaceEncodingInfo faceEncodingInfo = GetFromDB(imageFile);
            //if (faceEncodingInfo != null)
            //    throw new Exception($"{imageFile} уже есть в базе!");

            if (faceEncodingInfo == null)
                faceEncodingInfo = new FaceEncodingInfo(imageFile);

            var fingerAndLocations  = _fingerCollection.Find(f => f.Left == left
                && f.Right == right
                && f.Top == top
                && f.Bottom == bottom
                && f.FingerPrint.Equals(doubleInfo));

            FingerAndLocation fingerAndLocation;
            if (fingerAndLocations.Any())
                fingerAndLocation = fingerAndLocations.Single();
            else
            {
                fingerAndLocation = new FingerAndLocation();
                fingerAndLocation.FingerPrint = doubleInfo;
                fingerAndLocation.Left = left;
                fingerAndLocation.Right = right;
                fingerAndLocation.Top = top;
                fingerAndLocation.Bottom = bottom;

                _fingerCollection.Insert(fingerAndLocation);
            }

            //if (!faceEncodingInfo.FingerAndLocations.Any(fe => fe.Equals(fingerAndLocation)))
            if (!faceEncodingInfo.FingerAndLocations.Any(fe => fe.Bottom == fingerAndLocation.Bottom
            && fe.Left == fingerAndLocation.Left
            && fe.Right == fingerAndLocation.Right
            && fe.Top == fingerAndLocation.Top
            && fe.FingerPrint.Equals(fingerAndLocation.FingerPrint)))
                faceEncodingInfo.FingerAndLocations.Add(fingerAndLocation);
            try
            {
                _faceCollection.Upsert(faceEncodingInfo);
            }
            catch (Exception ex)
            {

                throw;
            }

            //var info = _faceCollection.Include(x => x.FingerAndLocations).FindById(imageFile);
            //if (info.FingerAndLocations.First().Left != faceEncodingInfo.FingerAndLocations.First().Left)
            //    throw new Exception("Данные не сохранились!");
        }

        public void AddFileWithoutFace(string imageFile)
        {
            FaceEncodingInfo faceEncodingInfo = GetFromDB(imageFile);
            if (faceEncodingInfo != null)
                throw new Exception($"{imageFile} уже есть в базе!");

            faceEncodingInfo = new FaceEncodingInfo(imageFile);
            _faceCollection.Upsert(faceEncodingInfo);
        }
    }
}

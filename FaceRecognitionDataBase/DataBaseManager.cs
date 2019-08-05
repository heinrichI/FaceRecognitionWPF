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


        public DataBaseManager(string dataBaseName)
        {
            ////// Re-use mapper from global instance
            var mapper = BsonMapper.Global;

            //// "Products" and "Customer" are from other collections (not embedded document)
            mapper.Entity<FaceEncodingInfo>()
                .DbRef(x => x.FingerAndLocations, "FingerAndLocations")    // 1 to Many reference
                .Id(f => f.Path);

            _db = new LiteDatabase(dataBaseName);

            _faceCollection = _db.GetCollection<FaceEncodingInfo>("FaceEncodingInfo");
            _faceCollection.EnsureIndex(x => x.Path);
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        public FaceEncodingInfo GetFromDB(string imageFile)
        {
            var info = _faceCollection.FindById(imageFile);
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
            if (faceEncodingInfo != null)
                throw new Exception($"{imageFile} уже есть в базе!");

            faceEncodingInfo = new FaceEncodingInfo(imageFile);
            FingerAndLocation fl = new FingerAndLocation();
            fl.FingerPrint = doubleInfo;
            fl.Left = left;
            fl.Right = right;
            fl.Top = top;
            fl.Bottom = bottom;

            faceEncodingInfo.FingerAndLocations.Add(fl);
            try
            {
                _faceCollection.Upsert(faceEncodingInfo);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

    }
}

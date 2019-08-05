using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionBusinessLogic.DataBase
{
    public interface IDataBaseManager : IDisposable
    {
        FaceEncodingInfo GetFromDB(string imageFile);

        void AddFaceInfo(string imageFile, double[] doubleInfo, 
            int left, int right, int top, int bottom);
    }
}

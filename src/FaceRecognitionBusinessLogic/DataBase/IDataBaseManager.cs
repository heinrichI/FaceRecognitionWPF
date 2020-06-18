using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionBusinessLogic.DataBase
{
    public interface IDataBaseManager : IDisposable
    {
        FaceInfo GetFromDB(string imageFile);

        /// <summary>
        /// Add face information to data base.
        /// </summary>
        /// <param name="imageFile"></param>
        /// <param name="doubleInfo"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="top"></param>
        /// <param name="bottom"></param>
        void AddFaceInfo(string imageFile, double[] doubleInfo, 
            int left, int right, int top, int bottom);

        void AddFileWithoutFace(string imageFile);

        IEnumerable<PathInfo> GetAll();

        int Remove(string path);

        bool UpsertFaceInfo(PathInfo info);

        long Shrink();
    }
}

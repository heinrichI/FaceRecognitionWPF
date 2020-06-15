using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FaceRecognitionBusinessLogic;
using FaceRecognitionBusinessLogic.DataBase;

namespace FaceRecognitionWPF
{
    /*class RemoveRecordForUnexistFilesManager
    {
        private IDataBaseManager _db;
        private IProgress<ProgressPartialResult> _progress;

        IEnumerable<PathInfo> _dbInfo;
        int _progressMaximum;

        public RemoveRecordForUnexistFilesManager(IDataBaseManager db, IProgress<ProgressPartialResult> progress)
        {
            _db = db;
            _progress = progress;
        }

        internal string Remove()
        {
            _progress.Report(new ProgressPartialResult() { Current = 0, Total = 0, Text = "Read data base" });

            _dbInfo = _db.GetAll().ToList();
            _progressMaximum = _dbInfo.Count();

            int removed = 0;
            int current = 0;
            foreach (var item in _dbInfo)
            {
                current++;
                _progress.Report(new ProgressPartialResult() { Current = current, Total = _progressMaximum, Text = item.Path });

                if (!System.IO.File.Exists(item.Path))
                {
                    removed++;
                    _db.Remove(item.Path);
                }
            }

            long shrinked = _db.Shrink();

            return $"Removed {removed}, shrinked {shrinked}";
        }
    }*/
}

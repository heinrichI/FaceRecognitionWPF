using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FaceRecognitionBusinessLogic;
using FaceRecognitionBusinessLogic.DataBase;

namespace FaceRecognitionWPF
{
    /*class ConvertToLowerCaseManager 
    {
        private IDataBaseManager _db;
        private IProgress<ProgressPartialResult> _progress;
        IEnumerable<PathInfo> _dbInfo;
        int _progressMaximum;

        public ConvertToLowerCaseManager(IDataBaseManager db, IProgress<ProgressPartialResult> progress)
        {
            _db = db;
            _progress = progress;
        }

        public string Convert()
        {
            _progress.Report(new ProgressPartialResult() { Current = 0, Total = 0, Text = "Read data base" });

            _dbInfo = _db.GetAll().ToList();
            _progressMaximum = _dbInfo.Count();

            int duplicate = 0;
            int renamed = 0;
            int current = 0;
            foreach (var item in _dbInfo)
            {
                current++;
                _progress.Report(new ProgressPartialResult() { Current = current, Total = _progressMaximum, Text = item.Path });

                var totalDupl = _dbInfo.Where(info => info.Path.ToLower() == item.Path.ToLower()).ToArray();
                if (totalDupl.Length > 1)
                {
                    duplicate += totalDupl.Length;
                    for (int i = 0; i < totalDupl.Length; i++)
                    {
                        _db.Remove(totalDupl[i].Path);
                    }
                    totalDupl[0].Path = item.Path.ToLower();
                    _db.UpsertFaceInfo(totalDupl[0]);
                }
                else
                {
                    if (item.Path != item.Path.ToLower())
                    //if (!item.Path.Equals(item.Path.ToLower(), StringComparison.CurrentCulture))
                    {
                        _db.Remove(item.Path);
                        renamed++;
                        item.Path = item.Path.ToLower();
                        _db.UpsertFaceInfo(item);
                    }
                }
            }

            return $"Renamed {renamed}, duplicate {duplicate}";
        }

    }*/
}

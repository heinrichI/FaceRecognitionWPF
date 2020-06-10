using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using FaceRecognitionBusinessLogic;
using FaceRecognitionBusinessLogic.DataBase;
using FaceRecognitionDotNet;

namespace FaceRecognitionWPF
{
    class CheckManager : BaseManager
    {
        static object _checkQueueLocker = new object();
        static object _progressLocker = new object();

        Queue<PathInfo> _checkQueue;
        IConfiguration _configuration;

        IProgress<ProgressPartialResult> _progress;
        int _progressMaximum;
        int _current = 0;

        public CheckManager(IDataBaseManager db, IConfiguration configuration,
             IProgress<ProgressPartialResult> progress)
        {
            _configuration = configuration;
            _progress = progress;

            var allInfo = db.GetAll().ToList();
            _progressMaximum = allInfo.Count();
            _checkQueue = new Queue<PathInfo>(allInfo);

            base.StartThreads(configuration.ThreadCount);
        }

        protected override void ThreadWork()
        {
            PathInfo faceEncodingInfo;

            using (var faceRecognition = FaceRecognition.Create(_configuration.ModelsDirectory))
            {
                while (true)
                {
                    lock (_checkQueueLocker)
                    {
                        if (_checkQueue.Count > 0)
                            faceEncodingInfo = _checkQueue.Dequeue();
                        else
                            return;
                    }

                    _progress.Report(new ProgressPartialResult()
                    {
                        Current = _current,
                        Total = _progressMaximum,
                        Text = faceEncodingInfo.Path
                    });
                    lock (_progressLocker)
                    {
                        _current++;
                    }

                    //load image
                    using (var loadedImage = FaceRecognition.LoadImageFile(faceEncodingInfo.Path))
                    {
                        //find face locations
                        var locations = faceRecognition.FaceLocations(loadedImage).ToArray();
                        //# If no faces are found in the image, return an empty result.
                        if (!locations.Any())
                        {
                            if (faceEncodingInfo.FingerAndLocations.Count() != locations.Count())
                                throw new Exception("faceEncodingInfo.FingerAndLocations.Count() != locations.Count()");
                            continue;
                        }

                        for (int i = 0; i < locations.Count(); i++)
                        {
                            var encodings = faceRecognition.FaceEncodings(loadedImage, new Location[] { locations[i] });
                            if (encodings == null)
                                continue;

                            var encoding = encodings.Single();

                            var info = new SerializationInfo(typeof(double), _formatterConverter);
                            encoding.GetObjectData(info, _context);
                            encoding.Dispose();

                            double[] checkedData = (double[])info.GetValue("_Encoding", typeof(double[]));

                            if (!faceEncodingInfo.FingerAndLocations[i].FingerPrint.SequenceEqual(checkedData))
                                throw new Exception($"For file {faceEncodingInfo.Path} data not equal!");

                        }
                    }
                }

            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using FaceRecognitionBusinessLogic;
using FaceRecognitionBusinessLogic.DataBase;
using FaceRecognitionBusinessLogic.KNN;
using FaceRecognitionBusinessLogic.ObjectModel;
using FaceRecognitionDotNet;

namespace FaceRecognitionWPF
{
    class TrainManager : BaseManager
    {
        static object _searchStackLocker = new object();
        static object _dbLocker = new object();
        static object _progressLocker = new object();
        static object _trainedInfoLocker = new object();

        private List<ClassInfo> _trainedInfo;
        IEnumerable<string> _classes;

        private IConfiguration _configuration;
        private IDataBaseManager _db;
        private IProgress<ProgressPartialResult> _progress;
        private IUiService _uiService;

        Queue<string> _searchQueue;
        int _progressMaximum;
        int _current = 0;

        public TrainManager(ref List<ClassInfo> trainedInfo,
            IConfiguration configuration,
            IDataBaseManager db,
            IProgress<ProgressPartialResult> progress,
            IUiService uiService,
            CancellationToken cancellationToken)
            : base(cancellationToken)
        {
            this._trainedInfo = trainedInfo;
            _configuration = configuration;
            this._db = db;
            _progress = progress;
            _uiService = uiService;
        }

        public IEnumerable<string> Train(int threadCount)
        {
            _progress.Report(new ProgressPartialResult() { Current = 0, Total = 0, Text = "Read images in directory" });

            var directories = System.IO.Directory.GetDirectories(_configuration.TrainPath);
            _classes = directories.Select(d => new DirectoryInfo(d).Name);

            var searchFiles = System.IO.Directory.GetFiles(_configuration.TrainPath, "*", SearchOption.AllDirectories);
            _progressMaximum = searchFiles.Count();
            _searchQueue = new Queue<string>(searchFiles);

            StartThreads(threadCount);

            _progress.Report(new ProgressPartialResult() { Current = _progressMaximum, Total = _progressMaximum, Text = String.Empty });

            return _classes;
        }

        protected override void ThreadWork()
        {
            string imagePath;

            FaceRecognition faceRecognition = null;
            while (true)
            {
                if (_cancellationToken.IsCancellationRequested)
                    break;

                lock (_searchStackLocker)
                {
                    if (_searchQueue.Count > 0)
                        imagePath = _searchQueue.Dequeue();
                    else
                        break;
                }

                _progress.Report(new ProgressPartialResult() { Current = _current, Total = _progressMaximum, Text = imagePath });
                lock (_progressLocker)
                {
                    _current++;
                }

                FaceRecognitionBusinessLogic.DataBase.FaceInfo founded;
                lock (_dbLocker)
                {
                    founded = _db.GetFromDB(imagePath);
                }
                if (founded == null)
                {
                    if (faceRecognition == null)
                        faceRecognition = FaceRecognition.Create(_configuration.ModelsDirectory);

                    FaceRecognitionDotNet.Image image;
                    try
                    {
                        image = FaceRecognition.LoadImageFile(imagePath);
                    }
                    catch (Exception ex)
                    {
                        _uiService.ShowMessage($"{ex.Message} \n {ex.StackTrace} \n {ex?.InnerException?.Message}",
                            "Exception on LoadImageFile");
                        continue;
                    }
                    using (image)
                    {
                        Debug.WriteLine($"Train on {imagePath}");
                        //find face locations
                        var faceBoundingBoxes = faceRecognition.FaceLocations(image, 1, Model.Hog);

                        var countOfFace = faceBoundingBoxes.Count();
                        if (countOfFace == 0)
                        {
                            _uiService.ShowFaceWindow(imagePath, Enumerable.Empty<FaceLocation>());
                            continue;
                            //throw new Exception($"Not founded face in {imageFile}");
                        }

                        if (countOfFace > 1)
                        {
                            var boxes = faceBoundingBoxes
                                .Select(l => new FaceLocation(l.Left, l.Right, l.Top, l.Bottom))
                                .ToList();
                            _uiService.ShowFaceWindow(imagePath, boxes);

                            continue;
                            //If there are no people (or too many people) in a training image, skip the image.
                            //throw new Exception($"Faces {countOfFace} > 1 in {imageFile}");
                        }
                        else
                        {
                            // Add face encoding for current image to the training set
                            var encodings = faceRecognition.FaceEncodings(image, faceBoundingBoxes);
                            if (encodings == null)
                                continue;

                            foreach (var encoding in encodings)
                            {
                                var info = new System.Runtime.Serialization.SerializationInfo(typeof(double), _formatterConverter);
                                encoding.GetObjectData(info, _context);

                                double[] doubleInfo = (double[])info.GetValue("_Encoding", typeof(double[]));
                                encoding.Dispose();
                                var dir = Path.GetDirectoryName(imagePath);
                                string directory = new DirectoryInfo(dir).Name;
                                lock (_trainedInfoLocker)
                                {
                                    _trainedInfo.Add(new ClassInfo(directory, doubleInfo, imagePath));
                                }

                                lock (_dbLocker)
                                {
                                    _db.AddFaceInfo(imagePath, doubleInfo, faceBoundingBoxes.Single().Left, faceBoundingBoxes.Single().Right,
                                    faceBoundingBoxes.Single().Top, faceBoundingBoxes.Single().Bottom);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Debug.WriteLine($"File {imagePath} in db");
                    var dir = Path.GetDirectoryName(imagePath);
                    string directory = new DirectoryInfo(dir).Name;
                    lock (_trainedInfoLocker)
                    {
                        var fingerAndLocation = founded.FingerAndLocations.Single();
                        _trainedInfo.Add(new ClassInfo(directory,
                            fingerAndLocation.FingerPrint, imagePath));
                    }
                }
            }

            if (faceRecognition != null)
                faceRecognition.Dispose();
        }
    }
}
using FaceRecognitionBusinessLogic;
using FaceRecognitionBusinessLogic.DataBase;
using FaceRecognitionBusinessLogic.KNN;
using FaceRecognitionBusinessLogic.ObjectModel;
using FaceRecognitionDotNet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace FaceRecognitionWPF
{
    public class SearchManager : BaseManager
    {
        static object _searchQueueLocker = new object();
        static object _dbLocker = new object();
        static object _progressLocker = new object();

        //Stack<string> _searchStack;
        Queue<string> _searchQueue;
        IProgress<ProgressPartialResult> _progress;
        int _progressMaximum;
        int _current = 0;

        IDataBaseManager _db;
        IConfiguration _configuration;
        List<ClassInfo> _trainedInfo;
        IEnumerable<string> _classes;
        ObservableCollection<DirectoryWithFaces> _directoryWithFaces;
        Action<string, ObservableCollection<DirectoryWithFaces>,
            VoteAndDistance, int, int, int, int, int> _addToViewImageAction;
        string _checkClass;

        public SearchManager(int threadCount,
            IConfiguration configuration,
            IProgress<ProgressPartialResult> progress,
            IDataBaseManager db, 
            List<ClassInfo> trainedInfo, 
            IEnumerable<string> classes,
            ObservableCollection<DirectoryWithFaces> directoryWithFaces,
            Action<string, ObservableCollection<DirectoryWithFaces>, 
                VoteAndDistance, int, int, int, int, int> addToViewImageAction,
            string checkClass = null)
        {
            _progress = progress;
            _db = db;
            _configuration = configuration;
            _trainedInfo = trainedInfo;
            _classes = classes;
            _directoryWithFaces = directoryWithFaces;
            _addToViewImageAction = addToViewImageAction;
            _checkClass = checkClass;

            _progress.Report(new ProgressPartialResult() { Current = 0, Total = 0, Text = "Read images in directory" });

            var ext = new List<string> { ".jpg", ".jpeg", ".png" };
            var searchFiles = System.IO.Directory.GetFiles(_configuration.SearchPath, "*", SearchOption.AllDirectories)
            .Where(s => ext.Contains(Path.GetExtension(s)));
            _progressMaximum = searchFiles.Count();
            //_searchStack = new Stack<string>(searchFiles);
            //_searchStack.Reverse();
            _searchQueue = new Queue<string>(searchFiles);

            base.StartThreads(threadCount);
        }

        protected override void ThreadWork()
        {
            string imagePath;

            using (var faceRecognition = FaceRecognition.Create(_configuration.ModelsDirectory))
            {
                while (true)
                {
                    lock (_searchQueueLocker)
                    {
                        if (_searchQueue.Count > 0)
                            imagePath = _searchQueue.Dequeue();
                        else
                            return;
                    }

                    _progress.Report(new ProgressPartialResult() { Current = _current, Total = _progressMaximum, Text = imagePath });
                    lock (_progressLocker)
                    { 
                        _current++;
                    }

                    //load image
                    //using (var ms = new MemoryStream(File.ReadAllBytes(imageFile3)))
                    FaceEncodingInfo founded;
                    lock (_dbLocker)
                    {
                        founded = _db.GetFromDB(imagePath);
                    }
                    if (founded == null)
                    {
                        FaceRecognitionDotNet.Image unknownImage;
                        try
                        {
                            unknownImage = FaceRecognition.LoadImageFile(imagePath);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"{ex.Message} \n {ex.StackTrace} \n {ex?.InnerException?.Message}", "Uncaught Thread Exception",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            continue;
                        }
                        using (unknownImage)
                        {
                            Debug.WriteLine($"Read {imagePath}");
                            //find face locations
                            var locations = faceRecognition.FaceLocations(unknownImage);
                            //# If no faces are found in the image, return an empty result.
                            if (!locations.Any())
                            {
                                Debug.WriteLine($"In {imagePath} not found faces");
                                lock (_dbLocker)
                                {
                                    _db.AddFileWithoutFace(imagePath);
                                }
                                continue;
                            }

                            foreach (var location in locations)
                            {
                                var encodings = faceRecognition.FaceEncodings(unknownImage, new Location[] { location });
                                if (encodings == null)
                                    continue;

                                var encoding = encodings.Single();

                                var info = new SerializationInfo(typeof(double), _formatterConverter);
                                encoding.GetObjectData(info, _context);
                                encoding.Dispose();

                                double[] unknown = (double[])info.GetValue("_Encoding", typeof(double[]));
                                lock (_dbLocker)
                                {
                                    _db.AddFaceInfo(imagePath, unknown, location.Left, location.Right,
                                        location.Top, location.Bottom);
                                }

                                VoteAndDistance predict = MyKnn.Classify(unknown, _trainedInfo, _classes.ToArray(), 1);

                                if (String.IsNullOrEmpty(_checkClass))
                                {
                                    if (predict.Distance < _configuration.DistanceThreshold)
                                    {
                                        Debug.WriteLine($"Found {predict.Name} in {imagePath} with {predict.Distance} distance");
                                        _addToViewImageAction(imagePath, _directoryWithFaces, predict, location.Left, location.Top,
                                                        location.Right - location.Left, location.Bottom - location.Top,
                                                        locations.Count());
                                    }
                                }
                                else
                                {
                                    if (predict.Distance > _configuration.DistanceThreshold
                                        || predict.Name != _checkClass)
                                    {
                                        _addToViewImageAction(imagePath, _directoryWithFaces, predict, location.Left, location.Top,
                                                        location.Right - location.Left, location.Bottom - location.Top,
                                                        locations.Count());
                                    }
                                }

                            }
                        }
                    }
                    else
                    {
                        foreach (var fingerAndLocations in founded.FingerAndLocations)
                        {
                            VoteAndDistance predict = MyKnn.Classify(fingerAndLocations.FingerPrint,
                                _trainedInfo, _classes.ToArray(), 1);

                            if (String.IsNullOrEmpty(_checkClass))
                            {
                                if (predict.Distance < _configuration.DistanceThreshold)
                                {
                                    _addToViewImageAction(imagePath, _directoryWithFaces, predict,
                                        fingerAndLocations.Left, fingerAndLocations.Top,
                                        fingerAndLocations.Right - fingerAndLocations.Left,
                                        fingerAndLocations.Bottom - fingerAndLocations.Top,
                                        founded.FingerAndLocations.Count);
                                }
                            }
                            else
                            {
                                if (predict.Distance > _configuration.DistanceThreshold
                                    || predict.Name != _checkClass)
                                {
                                    _addToViewImageAction(imagePath, _directoryWithFaces, predict, 
                                        fingerAndLocations.Left, fingerAndLocations.Top,
                                        fingerAndLocations.Right - fingerAndLocations.Left,
                                        fingerAndLocations.Bottom - fingerAndLocations.Top,
                                        founded.FingerAndLocations.Count);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

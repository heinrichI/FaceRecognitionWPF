using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using FaceRecognitionBusinessLogic;
using FaceRecognitionBusinessLogic.DataBase;
using FaceRecognitionBusinessLogic.KNN;
using FaceRecognitionBusinessLogic.ObjectModel;
using FaceRecognitionDotNet;
using FaceRecognitionWPF.View;
using FaceRecognitionWPF.ViewModel;

namespace FaceRecognitionWPF
{
    class TrainManager : BaseManager
    {
        static object _searchStackLocker = new object();
        static object _dbLocker = new object();
        static object _progressLocker = new object();
        static object _trainedInfoLocker = new object();

        private List<ClassInfo> _trainedInfo;
        private IConfiguration _configuration;
        private IDataBaseManager _db;
        private IProgress<ProgressPartialResult> _progress;
        View.WindowService _windowService;

        Queue<string> _searchQueue;
        int _progressMaximum;
        int _current = 0;

        public TrainManager(ref List<ClassInfo> trainedInfo, 
            out IEnumerable<string> classes, 
            int threadCount, 
            IConfiguration configuration, 
            IDataBaseManager db, 
            IProgress<ProgressPartialResult> progress,
            View.WindowService windowService)
        {
            this._trainedInfo = trainedInfo;
            classes = null;
            _configuration = configuration;
            this._db = db;
            _progress = progress;
            _windowService = windowService;

            _progress.Report(new ProgressPartialResult() { Current = 0, Total = 0, Text = "Read images in directory" });

            var directories = System.IO.Directory.GetDirectories(_configuration.TrainPath);
            classes = directories.Select(d => new DirectoryInfo(d).Name);

            var searchFiles = System.IO.Directory.GetFiles(_configuration.TrainPath, "*", 
                SearchOption.AllDirectories);
            _progressMaximum = searchFiles.Count();
            _searchQueue = new Queue<string>(searchFiles);

            base.StartThreads(threadCount);
        }

        public override void ThreadWork()
        {
            string imagePath;

            FaceRecognition faceRecognition = null;
                while (true)
                {
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

                    FaceEncodingInfo founded;
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
                            MessageBox.Show($"{ex.Message} \n {ex.StackTrace} \n {ex?.InnerException?.Message}",
                                "Exception on LoadImageFile",
                                MessageBoxButton.OK, MessageBoxImage.Error);
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
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    FaceViewModel vm = new FaceViewModel(imagePath);
                                    _windowService.ShowDialogWindow<FaceWindow>(vm);
                                });
                                continue;
                                //throw new Exception($"Not founded face in {imageFile}");
                            }

                            if (countOfFace > 1)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    FaceViewModel vm = new FaceViewModel(faceBoundingBoxes, imagePath);
                                    _windowService.ShowDialogWindow<FaceWindow>(vm);
                                });

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
                                    var info = new SerializationInfo(typeof(double), _formatterConverter);
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

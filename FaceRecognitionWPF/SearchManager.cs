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
    public class SearchManager
    {
        static object _searchStackLocker = new object();
        Stack<string> _searchStack;
        IProgress<ProgressPartialResult> _progress;
        int _progressMaximum;
        IDataBaseManager _db;
        string _modelsDirectory;
        IFormatterConverter _formatterConverter = new FormatterConverter();
        StreamingContext _context = new StreamingContext();
        List<ClassInfo> _trainedInfo;
        IEnumerable<string> _classes;
        double _distanceThreshold;
        Action<string, ObservableCollection<DirectoryWithFaces>,
            VoteAndDistance, int, int, int, int> _addToViewImageAction;

        public SearchManager(int threadCount, string searchPath, IProgress<ProgressPartialResult> progress,
            IDataBaseManager db, string modelsDirector, List<ClassInfo> trainedInfo, IEnumerable<string> classes,
            double distanceThreshold, Action<string, ObservableCollection<DirectoryWithFaces>,
            VoteAndDistance, int, int, int, int> addToViewImageAction)
        {
            _progress = progress;
            _db = db;
            _modelsDirectory = modelsDirector;
            _trainedInfo = trainedInfo;
            _classes = classes;
            _distanceThreshold = distanceThreshold;
            _addToViewImageAction = addToViewImageAction;

            var ext = new List<string> { ".jpg", ".jpeg", ".png" };
            var searchFiles = System.IO.Directory.GetFiles(searchPath, "*", SearchOption.AllDirectories)
            .Where(s => ext.Contains(Path.GetExtension(s)));
            _progressMaximum = searchFiles.Count();
            _searchStack = new Stack<string>(searchFiles);

            Thread[] threads = new Thread[threadCount];

            for (int i = 0; i < threadCount; i++)
            {
                threads[i] = new Thread(ThreadWork);
                threads[i].Start();
            }

            for (int i = 0; i < threadCount; i++)
            {
                threads[i].Join();
            }
        }

        public void ThreadWork()
        {
            string imagePath;

            using (var faceRecognition = FaceRecognition.Create(_modelsDirectory))
            {
                while (true)
                {
                    lock (_searchStackLocker)
                    {
                        if (_searchStack.Count > 0)
                            imagePath = _searchStack.Pop();
                        else
                            return;
                    }


                    int current = 0;

                    _progress.Report(new ProgressPartialResult() { Current = current, Total = _progressMaximum, Text = imagePath });
                    current++;

                    //load image
                    //using (var ms = new MemoryStream(File.ReadAllBytes(imageFile3)))
                    FaceEncodingInfo founded = _db.GetFromDB(imagePath);
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
                        //using (var unknownImage = FaceRecognition.LoadImage(ms))
                        {
                            Debug.WriteLine($"Read {imagePath}");
                            //find face locations
                            var locations = faceRecognition.FaceLocations(unknownImage);
                            //# If no faces are found in the image, return an empty result.
                            if (!locations.Any())
                            {
                                Debug.WriteLine($"In {imagePath} not found faces");
                                _db.AddFileWithoutFace(imagePath);
                                continue;
                            }

                            foreach (var location in locations)
                            {
                                var encodings = faceRecognition.FaceEncodings(unknownImage, new Location[] { location });
                                if (encodings == null)
                                    continue;

                                var encoding = encodings.Single();
                                //foreach (var encoding in encodings)

                                var info = new SerializationInfo(typeof(double), _formatterConverter);
                                encoding.GetObjectData(info, _context);
                                encoding.Dispose();

                                double[] unknown = (double[])info.GetValue("_Encoding", typeof(double[]));
                                _db.AddFaceInfo(imagePath, unknown, location.Left, location.Right,
                                    location.Top, location.Bottom);

                                VoteAndDistance predict = MyKnn.Classify(unknown, _trainedInfo, _classes.ToArray(), 1);

                                if (predict.Distance < _distanceThreshold)
                                {
                                    Debug.WriteLine($"Found {predict.Name} in {imagePath} with {predict.Distance} distance");
                                    _addToViewImageAction(imagePath, DirectoriesWithFaces, predict, location.Left, location.Top,
                                                    location.Right - location.Left, location.Bottom - location.Top);
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

                            if (predict.Distance < _distanceThreshold)
                            {
                                _addToViewImageAction(imagePath, DirectoriesWithFaces, predict,
                                    fingerAndLocations.Left, fingerAndLocations.Top,
                                    fingerAndLocations.Right - fingerAndLocations.Left,
                                    fingerAndLocations.Bottom - fingerAndLocations.Top);

                            }
                        }
                    }
                }
            }
        }
    }
}

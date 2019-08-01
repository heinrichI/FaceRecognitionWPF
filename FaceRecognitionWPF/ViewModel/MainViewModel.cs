using FaceRecognitionDotNet;
using FaceRecognitionWPF.KNN;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionWPF.ViewModel 
{
    class MainViewModel : BasePropertyChanged
    {
        public MainViewModel()
        {
            TrainPath = @"d:\Борисов\WallpapersSort\TrainFace";
            SearchPath = @"d:\Борисов\WallpapersSort\body";
            ModelsDirectory = @"d:\face_recognition_models";
            DistanceThreshold = 0.6;
        }

        string _trainPath;
        public string TrainPath
        {
            get => this._trainPath;
            set
            {
                this._trainPath = value;
                this.RaisePropertyChangedEvent("TrainPath");
            }
        }

        string _searchPath;
        public string SearchPath
        {
            get => this._searchPath;
            set
            {
                this._searchPath = value;
                this.RaisePropertyChangedEvent("SearchPath");
            }
        }

        string _modelsDirectory;
        public string ModelsDirectory
        {
            get => this._modelsDirectory;
            set
            {
                this._modelsDirectory = value;
                this.RaisePropertyChangedEvent("ModelsDirectory");
            }
        }

        
        double _distanceThreshold;
        public double DistanceThreshold
        {
            get => this._distanceThreshold;
            set
            {
                this._distanceThreshold = value;
                this.RaisePropertyChangedEvent("DistanceThreshold");
            }
        }

        public void OnClosing(object sender, CancelEventArgs e)
        {

        }

        private RelayCommand _runCommand;
        public RelayCommand RunCommand
        {
            get
            {
                return this._runCommand ?? (this._runCommand = new RelayCommand(async (arg) =>
                {
                    //var openFileDialog = new OpenFileDialog();
                    //var dialogResult = openFileDialog.ShowDialog();
                    //if (dialogResult != true)
                    //    return;

                    //var path = openFileDialog.FileName;

                    

                    await Task.Run(() =>
                    {
                        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                        DlibDotNet.Dlib.Encoding = System.Text.Encoding.GetEncoding(1251);

                        using (var faceRecognition = FaceRecognition.Create(ModelsDirectory))
                        {
                            var directories = System.IO.Directory.GetDirectories(TrainPath);
                            var classes = directories.Select(d => new DirectoryInfo(d).Name);

                            List<ClassInfo> trainedInfo = new List<ClassInfo>();
                            foreach (var directory in directories)
                            {
                                //ClassInfo ci = new ClassInfo(directory);
                                //trainedInfo.Add(ci);

                                var files = System.IO.Directory.GetFiles(directory);

                                foreach (var imageFile in files)
                                {
                                    using (var image = FaceRecognition.LoadImageFile(imageFile))
                                    {
                                        //var sw = new Stopwatch();
                                        //sw.Start();

                                        var encodings = faceRecognition.FaceEncodings(image);
                                        if (encodings == null)
                                            continue;

                                        foreach (var encoding in encodings)
                                        {
                                            IFormatterConverter formatterConverter = new FormatterConverter();
                                            var info = new SerializationInfo(typeof(double), formatterConverter);
                                            StreamingContext context = new StreamingContext();
                                            encoding.GetObjectData(info, context);

                                            double[] doubleInfo = (double[])info.GetValue("_Encoding", typeof(double[]));
                                            //ci.Data.AddRange(doubleInfo);
                                            trainedInfo.Add(new ClassInfo(new DirectoryInfo(directory).Name, doubleInfo));
                                            encoding.Dispose();
                                        }

                                        //sw.Stop();

                                        //var total = sw.ElapsedMilliseconds;
                                        //Console.WriteLine($"Total: {total} [ms]");
                                    }

                                    //using (var faceDetector = Dlib.GetFrontalFaceDetector())
                                    //using (var data = new MemoryStream(File.ReadAllBytes(imageFile)))
                                    //{
                                    //    // DlibDotNet can create Array2D from file but this sample demonstrate
                                    //    // converting managed image class to dlib class and vice versa.
                                    //    var bitmap = new WriteableBitmap(BitmapFrame.Create(data));
                                    //    using (var image = bitmap.ToArray2D<RgbPixel>())
                                    //    {
                                    //        var dets = faceDetector.Operator(image);
                                    //        foreach (var r in dets)
                                    //            Dlib.DrawRectangle(image, r, new RgbPixel { Green = 255 });

                                    //        var result = image.ToWriteableBitmap();
                                    //        if (result.CanFreeze)
                                    //            result.Freeze();
                                    //        Application.Current.Dispatcher.Invoke(() =>
                                    //        {
                                    //            this.Image = result;
                                    //        });
                                    //    }
                                    //}
                                }


                            }

                            var searchFiles = System.IO.Directory.GetFiles(SearchPath);

                            foreach (var imageFile in searchFiles)
                            {
                                using (var ms = new MemoryStream(File.ReadAllBytes(imageFile)))
                                using (var unknownImage = FaceRecognition.LoadImageFile(imageFile))
                                //using (var unknownImage = FaceRecognition.LoadImage(ms))
                                {
                                    var encodings = faceRecognition.FaceEncodings(unknownImage);
                                    if (encodings == null)
                                        continue;

                                    foreach (var encoding in encodings)
                                    {
                                        IFormatterConverter formatterConverter = new FormatterConverter();
                                        var info = new SerializationInfo(typeof(double), formatterConverter);
                                        StreamingContext context = new StreamingContext();
                                        encoding.GetObjectData(info, context);

                                        double[] unknown = (double[])info.GetValue("_Encoding", typeof(double[]));
                                        VoteAndDistance predict = MyKnn.Classify(unknown, trainedInfo, classes.ToArray(), 1);
                                        encoding.Dispose();
                                        if (predict.Distance > DistanceThreshold)
                                            Debug.WriteLine($"Found {predict.Name} in {imageFile} with {predict.Distance} distance");
                                    }

                                    //int predicted = KNN.KNN.Classify(unknown, trainData, numClasses, k);

                                }
                            }
                            // FaceRecognition.CompareFaces()
                        }
                    })
                    .ContinueWith(c =>
                    {
                        AggregateException exception = c.Exception;

                        System.Windows.MessageBox.Show($"{exception.Message} \n {exception.StackTrace}",
                            "Uncaught Thread Exception", System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Error);
                    }, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
                }, (arg) => true));
            }
        }
    }
}

using DlibDotNet;
using DlibDotNet.Extensions;
using FaceRecognitionBusinessLogic;
using FaceRecognitionBusinessLogic.DataBase;
using FaceRecognitionBusinessLogic.ObjectModel;
using FaceRecognitionDotNet;
using FaceRecognitionWPF.KNN;
using FaceRecognitionWPF.View;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FaceRecognitionWPF.ViewModel
{
    class MainViewModel : BasePropertyChanged
    {
        public MainViewModel(View.WindowService windowService)
        {
            _windowService = windowService;

            TrainPath = @"c:\FaceTrain";
            SearchPath = @"d:\Downloads3\body";
            ModelsDirectory = @"h:\Face Recognition\models";
            DistanceThreshold = 0.6;

            DirectoriesWithFaces = new ObservableCollection<DirectoryWithFaces>();
        }

        View.WindowService _windowService;

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

        ObservableCollection<DirectoryWithFaces> _directoriesWithFaces;
        public ObservableCollection<DirectoryWithFaces> DirectoriesWithFaces
        {
            get => _directoriesWithFaces;
            set
            {
                this._directoriesWithFaces = value;
                this.OnPropertyChanged();
            }
        }


        public void OnClosing(object sender, CancelEventArgs e)
        {

        }


        public int ThumbnailWidth
        {
            get => 150;
        }

        ICommand _openImageCommand;
        public ICommand OpenImageCommand
        {
            get
            {
                return _openImageCommand ?? (_openImageCommand = new RelayCommand(arg =>
                {
                    var path = (string)arg;
                    if (System.IO.File.Exists(path))
                        System.Diagnostics.Process.Start(path);
                }, arg => true));
            }
        }


        ImageSource _errorImage;
        public ImageSource ErrorImage
        {
            get => this._errorImage;
            set
            {
                this._errorImage = value;
                this.OnPropertyChanged();
            }
        }


        private RelayCommand _runCommand;
        public RelayCommand RunCommand
        {
            get
            {
                return this._runCommand ?? (this._runCommand = new RelayCommand(async (arg) =>
                {
                    await Task.Run(() =>
                    {
                        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                        DlibDotNet.Dlib.Encoding = System.Text.Encoding.GetEncoding(1251);

                        IFormatterConverter formatterConverter = new FormatterConverter();
                        StreamingContext context = new StreamingContext();

                        ////// Re-use mapper from global instance
                        var mapper = BsonMapper.Global;

                        //// "Products" and "Customer" are from other collections (not embedded document)
                        mapper.Entity<FaceEncodingInfo>()
                        //    .DbRef(x => x.FingerPrints, "FingerPrints")    // 1 to Many reference
                            .Id(f => f.Path);

                        using (var db = new LiteDatabase("faces.litedb"))
                        using (var faceRecognition = FaceRecognition.Create(ModelsDirectory))
                        {
                            var directories = System.IO.Directory.GetDirectories(TrainPath);
                            var classes = directories.Select(d => new DirectoryInfo(d).Name);

                            List<ClassInfo> trainedInfo = new List<ClassInfo>();
                            foreach (var directory in directories)
                            {
                                var files = System.IO.Directory.GetFiles(directory);

                                foreach (var imageFile in files)
                                {
                                    var faceCollection = db.GetCollection<FaceEncodingInfo>("FaceEncodingInfo");
                                    faceCollection.EnsureIndex(x => x.Path);

                                    FaceEncodingInfo founded = GetFromDB(faceCollection, imageFile);
                                    if (founded == null)
                                    {
                                        using (var image = FaceRecognition.LoadImageFile(imageFile))
                                        {
                                            Debug.WriteLine($"Train on {imageFile}");
                                            //var sw = new Stopwatch();
                                            //sw.Start();

                                            var faceBoundingBoxes = faceRecognition.FaceLocations(image, 1, Model.Hog);

                                            //Bitmap source = new Bitmap(imageFile);
                                            //Bitmap croppedImage = source.Clone(
                                            //    new System.Drawing.Rectangle(location.Left, location.Top,
                                            //    location.Right - location.Left, location.Bottom - location.Top), source.PixelFormat);

                                            var countOfFace = faceBoundingBoxes.Count();
                                            if (countOfFace == 0)
                                            {
                                                Application.Current.Dispatcher.Invoke(() =>
                                                {
                                                    FaceViewModel vm = new FaceViewModel(imageFile);
                                                    _windowService.ShowDialogWindow<FaceWindow>(vm);
                                                });
                                                continue;
                                                //throw new Exception($"Not founded face in {imageFile}");
                                            }

                                            if (countOfFace > 1)
                                            {
                                                Application.Current.Dispatcher.Invoke(() =>
                                                {
                                                    FaceViewModel vm = new FaceViewModel(faceBoundingBoxes, imageFile);
                                                    _windowService.ShowDialogWindow<FaceWindow>(vm);
                                                });

                                                continue;
                                                //If there are no people (or too many people) in a training image, skip the image.
                                                //throw new Exception($"Faces {countOfFace} > 1 in {imageFile}");
                                            }
                                            //print("Image {} not suitable for training: {}".format(img_path, "Didn't find a face" if len(face_bounding_boxes) < 1 else "Found more than one face"))
                                            else
                                            {
                                                // Add face encoding for current image to the training set
                                                var encodings = faceRecognition.FaceEncodings(image, faceBoundingBoxes);
                                                if (encodings == null)
                                                    continue;

                                                foreach (var encoding in encodings)
                                                {
                                                    //IFormatterConverter formatterConverter = new FormatterConverter();
                                                    var info = new SerializationInfo(typeof(double), formatterConverter);
                                                    //StreamingContext context = new StreamingContext();
                                                    encoding.GetObjectData(info, context);

                                                    double[] doubleInfo = (double[])info.GetValue("_Encoding", typeof(double[]));
                                                    //ci.Data.AddRange(doubleInfo);
                                                    trainedInfo.Add(new ClassInfo(new DirectoryInfo(directory).Name, doubleInfo));
                                                    encoding.Dispose();

                                                    FaceEncodingInfo faceEncodingInfo = new FaceEncodingInfo(imageFile);
                                                    faceEncodingInfo.FingerPrints.Add(doubleInfo);
                                                    try
                                                    {
                                                        faceCollection.Upsert(faceEncodingInfo);
                                                    }
                                                    catch (Exception ex)
                                                    {

                                                        throw;
                                                    }

                                                }

                                                //faceCollection.Insert()
                                            }

                                            //sw.Stop();

                                            //var total = sw.ElapsedMilliseconds;
                                            //Console.WriteLine($"Total: {total} [ms]");
                                        }
                                    }
                                    else
                                    {
                                        Debug.WriteLine($"File {imageFile} in db");
                                        trainedInfo.Add(new ClassInfo(new DirectoryInfo(directory).Name, founded.FingerPrints[0]));
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


                            var searchFiles = System.IO.Directory.GetFiles(SearchPath, "*", SearchOption.AllDirectories);
                            //string imageFile3 = @"d:\Борисов\WallpapersSort\body\-11072550_376068146.jpg";
                            foreach (var imageFile in searchFiles)
                            {
                                //load image
                                //using (var ms = new MemoryStream(File.ReadAllBytes(imageFile3)))
                                using (var unknownImage = FaceRecognition.LoadImageFile(imageFile))
                                //using (var unknownImage = FaceRecognition.LoadImage(ms))
                                {
                                    Debug.WriteLine($"Read {imageFile}");
                                    //find face locations
                                    var locations = faceRecognition.FaceLocations(unknownImage);
                                    //# If no faces are found in the image, return an empty result.
                                    if (!locations.Any())
                                    {
                                        Debug.WriteLine($"In {imageFile} not found faces");
                                        continue;
                                    }

                                    foreach (var location in locations)
                                    {
                                        var encodings = faceRecognition.FaceEncodings(unknownImage, locations);
                                        if (encodings == null)
                                            continue;

                                        foreach (var encoding in encodings)
                                        {
                                            var info = new SerializationInfo(typeof(double), formatterConverter);
                                            encoding.GetObjectData(info, context);

                                            double[] unknown = (double[])info.GetValue("_Encoding", typeof(double[]));
                                            VoteAndDistance predict = MyKnn.Classify(unknown, trainedInfo, classes.ToArray(), 1);
                                            encoding.Dispose();
                                            if (predict.Distance < DistanceThreshold)
                                            {
                                                Debug.WriteLine($"Found {predict.Name} in {imageFile} with {predict.Distance} distance");

                                                //this.Invoke(new Action(() =>
                                                Application.Current.Dispatcher.Invoke(() =>
                                                {
                                                    //Image = new BitmapImage(croppedImage);
                                                    try
                                                    {
                                                        BitmapImage src = new BitmapImage();
                                                        src.BeginInit();
                                                        src.UriSource = new Uri(imageFile, UriKind.Relative);
                                                        src.CacheOption = BitmapCacheOption.OnLoad;
                                                        src.EndInit();
                                                        //if (src.CanFreeze)
                                                        //    src.Freeze();
                                                        CroppedBitmap cropped = new CroppedBitmap(src, new Int32Rect(location.Left, location.Top,
                                                            location.Right - location.Left, location.Bottom - location.Top));

                                                        //this.Image = cropped;

                                                        string directory = Path.GetDirectoryName(imageFile);
                                                        var dirWithFaces = DirectoriesWithFaces
                                                            .SingleOrDefault(dir => dir.Name == directory);
                                                        if (dirWithFaces == null)
                                                        {
                                                            dirWithFaces = new DirectoryWithFaces(directory);
                                                            DirectoriesWithFaces.Add(dirWithFaces);
                                                        }

                                                        //var image = dirWithFaces.Images.SingleOrDefault(im => im.Path == imageFile);
                                                        //if (image == null)
                                                        //{
                                                        //    image = new ImageInfo(imageFile);
                                                        //    dirWithFaces.Images.Add(image);
                                                        //}
                                                        dirWithFaces.Faces.Add(new FaceInfo()
                                                        {
                                                            Image = cropped,
                                                            Path = imageFile,
                                                            Predict = predict.Name
                                                        });
                                                    }
                                                    catch (Exception ex)
                                                    {

                                                        throw;
                                                    }

                                                });

                                                //using (var data = new MemoryStream(File.ReadAllBytes(imageFile)))
                                                //{
                                                //    // DlibDotNet can create Array2D from file but this sample demonstrate
                                                //    // converting managed image class to dlib class and vice versa.
                                                //    var bitmap = new WriteableBitmap(BitmapFrame.Create(data));
                                                //    using (var image = bitmap.ToArray2D<RgbPixel>())
                                                //    {
                                                //        WriteableBitmap.Create()
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
                                            }
                                        }
                                    }

                                }

                                //int predicted = KNN.KNN.Classify(unknown, trainData, numClasses, k);
                            }
                        }
                        // FaceRecognition.CompareFaces()
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

        private FaceEncodingInfo GetFromDB(LiteCollection<FaceEncodingInfo> faceCollection, string imageFile)
        {
            var info = faceCollection.FindById(imageFile);
            if (info != null)
            {
                var fi = new FileInfo(imageFile);
                //Debug.WriteLine(fi.LastWriteTime.Ticks);
                if (fi.LastWriteTime.ToLongTimeString() == info.LastWriteTime.ToLongTimeString()
                    && fi.LastWriteTime.ToLongDateString() == info.LastWriteTime.ToLongDateString()
                    && fi.Length == info.Length)
                    return info;
                else
                    Debug.WriteLine($"Не совпадает LastWriteTime Length {imageFile}");
            }
            return null;
        }
    }
}

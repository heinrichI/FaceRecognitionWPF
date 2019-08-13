﻿using DlibDotNet;
using DlibDotNet.Extensions;
using FaceRecognitionBusinessLogic;
using FaceRecognitionBusinessLogic.DataBase;
using FaceRecognitionBusinessLogic.KNN;
using FaceRecognitionBusinessLogic.ObjectModel;
using FaceRecognitionDotNet;
using FaceRecognitionWPF.Helper;
using FaceRecognitionWPF.Menu;
using FaceRecognitionWPF.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TinyIoC;

namespace FaceRecognitionWPF.ViewModel
{
    class MainViewModel : BasePropertyChanged
    {
        public MainViewModel(View.WindowService windowService, IConfiguration configuration)
        {
            _windowService = windowService;
            _configuration = configuration;

            DirectoriesWithFaces = new ObservableCollection<DirectoryWithFaces>();

            _progress = new Progress<ProgressPartialResult>((result) =>
            {
                ProgressMaximum = result.Total;
                ProgressValue = result.Current;
                //ProgressValueTaskbar = (double)result.Current / (double)result.Total;
                ProgressText = String.Format("{0} / {1}  {2}", result.Current, result.Total, result.Text);
            });
        }


        public void OnClosing(object sender, CancelEventArgs e)
        {
            _configuration.Save();
        }

        const int _addBorder = 30;

        private IProgress<ProgressPartialResult> _progress;

        View.WindowService _windowService;

        IConfiguration _configuration;
        public IConfiguration Configuration
        {
            get { return _configuration; }
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


        public int ThumbnailWidth
        {
            get => 150;
        }

        int _threadCount = Environment.ProcessorCount;
        public int ThreadCount
        {
            get => this._threadCount;
            set
            {
                this._threadCount = value;
                this.OnPropertyChanged();
            }
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


        //ImageSource _errorImage;
        //public ImageSource ErrorImage
        //{
        //    get => this._errorImage;
        //    set
        //    {
        //        this._errorImage = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        int _progressValue;
        public int ProgressValue
        {
            get => this._progressValue;
            set
            {
                this._progressValue = value;
                this.OnPropertyChanged();
            }
        }

        int _progressMaximum;
        public int ProgressMaximum
        {
            get => this._progressMaximum;
            set
            {
                if (_progressMaximum != value)
                {
                    this._progressMaximum = value;
                    this.OnPropertyChanged();
                }
            }
        }

        string _progressText;
        public string ProgressText
        {
            get => this._progressText;
            set
            {
                this._progressText = value;
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
                    //IProgress<int> progress = new Progress<int>(percent =>
                    //{
                    //    Progress = percent;
                    //});

                    try
                    {

                        await Task.Run(() =>
                        {
                            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                            DlibDotNet.Dlib.Encoding = System.Text.Encoding.GetEncoding(1251);

                            IFormatterConverter formatterConverter = new FormatterConverter();
                            StreamingContext context = new StreamingContext();


                            using (var db = TinyIoC.TinyIoCContainer.Current.Resolve<IDataBaseManager>(
                                new NamedParameterOverloads() { { "dataBaseName", "faces.litedb" } }))
                            {
                                List<ClassInfo> trainedInfo = new List<ClassInfo>();
                                IEnumerable<string> classes;

                                TrainManager trainManager = new TrainManager(ref trainedInfo, out classes,
                                    ThreadCount, _configuration, db, _progress, _windowService);

                                //using (var faceRecognition = FaceRecognition.Create(_configuration.ModelsDirectory))
                                //{
                                //    var directories = System.IO.Directory.GetDirectories(_configuration.TrainPath);
                                //    classes = directories.Select(d => new DirectoryInfo(d).Name);

                                //    ProgressMaximum = directories.Length;
                                //    int current = 0;
                                //    foreach (var directory in directories)
                                //    {
                                //        ProgressValue = current;
                                //        current++;

                                //        var files = System.IO.Directory.GetFiles(directory);

                                //        foreach (var imageFile in files)
                                //        {
                                //            ProgressText = imageFile;

                                //            FaceEncodingInfo founded = db.GetFromDB(imageFile);
                                //            if (founded == null)
                                //            {
                                //                using (var image = FaceRecognition.LoadImageFile(imageFile))
                                //                {
                                //                    Debug.WriteLine($"Train on {imageFile}");
                                //                //var sw = new Stopwatch();
                                //                //sw.Start();

                                //                var faceBoundingBoxes = faceRecognition.FaceLocations(image, 
                                //                    1, Model.Hog);

                                //                //Bitmap source = new Bitmap(imageFile);
                                //                //Bitmap croppedImage = source.Clone(
                                //                //    new System.Drawing.Rectangle(location.Left, location.Top,
                                //                //    location.Right - location.Left, location.Bottom - location.Top), source.PixelFormat);

                                //                var countOfFace = faceBoundingBoxes.Count();
                                //                    if (countOfFace == 0)
                                //                    {
                                //                        Application.Current.Dispatcher.Invoke(() =>
                                //                        {
                                //                            FaceViewModel vm = new FaceViewModel(imageFile);
                                //                            _windowService.ShowDialogWindow<FaceWindow>(vm);
                                //                        });
                                //                        continue;
                                //                    //throw new Exception($"Not founded face in {imageFile}");
                                //                }

                                //                if (countOfFace > 1)
                                //                {
                                //                    Application.Current.Dispatcher.Invoke(() =>
                                //                    {
                                //                        FaceViewModel vm = new FaceViewModel(faceBoundingBoxes, imageFile);
                                //                        _windowService.ShowDialogWindow<FaceWindow>(vm);
                                //                    });

                                //                    continue;
                                //                    //If there are no people (or too many people) in a training image, skip the image.
                                //                    //throw new Exception($"Faces {countOfFace} > 1 in {imageFile}");
                                //                }
                                //                //print("Image {} not suitable for training: {}".format(img_path, "Didn't find a face" if len(face_bounding_boxes) < 1 else "Found more than one face"))
                                //                else
                                //                    {
                                //                    // Add face encoding for current image to the training set
                                //                    var encodings = faceRecognition.FaceEncodings(image, faceBoundingBoxes);
                                //                        if (encodings == null)
                                //                            continue;

                                //                        foreach (var encoding in encodings)
                                //                        {
                                //                            //IFormatterConverter formatterConverter = new FormatterConverter();
                                //                            var info = new SerializationInfo(typeof(double), formatterConverter);
                                //                            //StreamingContext context = new StreamingContext();
                                //                            encoding.GetObjectData(info, context);

                                //                            double[] doubleInfo = (double[])info.GetValue("_Encoding", typeof(double[]));
                                //                            //ci.Data.AddRange(doubleInfo);
                                //                            trainedInfo.Add(new ClassInfo(new DirectoryInfo(directory).Name, doubleInfo));
                                //                                encoding.Dispose();


                                //                                db.AddFaceInfo(imageFile, doubleInfo, faceBoundingBoxes.Single().Left, faceBoundingBoxes.Single().Right,
                                //                                    faceBoundingBoxes.Single().Top, faceBoundingBoxes.Single().Bottom);


                                //                        }

                                //                    //faceCollection.Insert()
                                //                }

                                //                //sw.Stop();

                                //                //var total = sw.ElapsedMilliseconds;
                                //                //Console.WriteLine($"Total: {total} [ms]");
                                //            }
                                //            }
                                //            else
                                //            {
                                //                Debug.WriteLine($"File {imageFile} in db");
                                //                trainedInfo.Add(new ClassInfo(new DirectoryInfo(directory).Name,
                                //                    founded.FingerAndLocations.Single().FingerPrint));
                                //            }

                                //        //using (var faceDetector = Dlib.GetFrontalFaceDetector())
                                //        //using (var data = new MemoryStream(File.ReadAllBytes(imageFile)))
                                //        //{
                                //        //    // DlibDotNet can create Array2D from file but this sample demonstrate
                                //        //    // converting managed image class to dlib class and vice versa.
                                //        //    var bitmap = new WriteableBitmap(BitmapFrame.Create(data));
                                //        //    using (var image = bitmap.ToArray2D<RgbPixel>())
                                //        //    {
                                //        //        var dets = faceDetector.Operator(image);
                                //        //        foreach (var r in dets)
                                //        //            Dlib.DrawRectangle(image, r, new RgbPixel { Green = 255 });

                                //        //        var result = image.ToWriteableBitmap();
                                //        //        if (result.CanFreeze)
                                //        //            result.Freeze();
                                //        //        Application.Current.Dispatcher.Invoke(() =>
                                //        //        {
                                //        //            this.Image = result;
                                //        //        });
                                //        //    }
                                //        //}
                                //    }
                                //}


                                //var ext = new List<string> { ".jpg", ".jpeg", ".png" };
                                //var searchFiles = System.IO.Directory.GetFiles(SearchPath, "*", SearchOption.AllDirectories)
                                //.Where(s => ext.Contains(Path.GetExtension(s)));
                                //    //string imageFile3 = @"d:\Борисов\WallpapersSort\body\-11072550_376068146.jpg";

                                //    int progressCount = 0;
                                //    ProgressMaximum = searchFiles.Count();
                                //foreach (var imageFile in searchFiles)
                                //    {
                                //        progress.Report(progressCount);
                                //        progressCount++;

                                //        //load image
                                //        //using (var ms = new MemoryStream(File.ReadAllBytes(imageFile3)))
                                //        FaceEncodingInfo founded = db.GetFromDB(imageFile);
                                //        if (founded == null)
                                //        {
                                //            FaceRecognitionDotNet.Image unknownImage;
                                //            try
                                //            {
                                //                unknownImage = FaceRecognition.LoadImageFile(imageFile);
                                //            }
                                //            catch (Exception ex)
                                //            {
                                //                MessageBox.Show($"{ex.Message} \n {ex.StackTrace} \n {ex?.InnerException?.Message}", "Uncaught Thread Exception",
                                //                    MessageBoxButton.OK, MessageBoxImage.Error);
                                //                continue;
                                //            }
                                //            using (unknownImage)
                                //        //using (var unknownImage = FaceRecognition.LoadImage(ms))
                                //        {
                                //                Debug.WriteLine($"Read {imageFile}");
                                //                //find face locations
                                //                var locations = faceRecognition.FaceLocations(unknownImage);
                                //                //# If no faces are found in the image, return an empty result.
                                //                if (!locations.Any())
                                //                {
                                //                    Debug.WriteLine($"In {imageFile} not found faces");
                                //                    db.AddFileWithoutFace(imageFile);
                                //                    continue;
                                //                }

                                //                foreach (var location in locations)
                                //                {
                                //                    var encodings = faceRecognition.FaceEncodings(unknownImage, new Location[] { location });
                                //                    if (encodings == null)
                                //                        continue;

                                //                    var encoding = encodings.Single();
                                //                    //foreach (var encoding in encodings)
                                                    
                                //                    var info = new SerializationInfo(typeof(double), formatterConverter);
                                //                    encoding.GetObjectData(info, context);
                                //                    encoding.Dispose();

                                //                    double[] unknown = (double[])info.GetValue("_Encoding", typeof(double[]));
                                //                    db.AddFaceInfo(imageFile, unknown, location.Left, location.Right,
                                //                        location.Top, location.Bottom);

                                //                    VoteAndDistance predict = MyKnn.Classify(unknown, trainedInfo, classes.ToArray(), 1);

                                //                    if (predict.Distance < DistanceThreshold)
                                //                    {
                                //                        Debug.WriteLine($"Found {predict.Name} in {imageFile} with {predict.Distance} distance");
                                //                        AddToViewImage(imageFile, DirectoriesWithFaces, predict, location.Left, location.Top,
                                //                                        location.Right - location.Left, location.Bottom - location.Top);
                                //                    }

                                //                    //using (var data = new MemoryStream(File.ReadAllBytes(imageFile)))
                                //                    //{
                                //                    //    // DlibDotNet can create Array2D from file but this sample demonstrate
                                //                    //    // converting managed image class to dlib class and vice versa.
                                //                    //    var bitmap = new WriteableBitmap(BitmapFrame.Create(data));
                                //                    //    using (var image = bitmap.ToArray2D<RgbPixel>())
                                //                    //    {
                                //                    //        WriteableBitmap.Create()
                                //                    //        var dets = faceDetector.Operator(image);
                                //                    //        foreach (var r in dets)
                                //                    //            Dlib.DrawRectangle(image, r, new RgbPixel { Green = 255 });

                                //                    //        var result = image.ToWriteableBitmap();
                                //                    //        if (result.CanFreeze)
                                //                    //            result.Freeze();
                                //                    //        Application.Current.Dispatcher.Invoke(() =>
                                //                    //        {
                                //                    //            this.Image = result;
                                //                    //        });
                                //                    //    }
                                                    
                                           
                                //                }
                                //            }
                                //        }
                                //        else
                                //        {
                                //            foreach (var fingerAndLocations in founded.FingerAndLocations)
                                //            {
                                //                VoteAndDistance predict = MyKnn.Classify(fingerAndLocations.FingerPrint,
                                //                    trainedInfo, classes.ToArray(), 1);

                                //                if (predict.Distance < DistanceThreshold)
                                //                {
                                //                    AddToViewImage(imageFile, DirectoriesWithFaces, predict,
                                //                        fingerAndLocations.Left, fingerAndLocations.Top,
                                //                        fingerAndLocations.Right - fingerAndLocations.Left,
                                //                        fingerAndLocations.Bottom - fingerAndLocations.Top);

                                //                }
                                //            }
                                //        }

                                    //int predicted = KNN.KNN.Classify(unknown, trainData, numClasses, k);
                                //}
                                //}

                                SearchManager sm = new SearchManager(ThreadCount, _configuration, _progress, 
                                    db, trainedInfo, classes, 
                                    DirectoriesWithFaces, AddToViewImage);
                            }



                            MessageBox.Show("Done!");
                    });
                    //.ContinueWith(c =>
                    //{
                    //    AggregateException exception = c.Exception;

                    //    System.Windows.MessageBox.Show($"{exception.Message} \n {exception.StackTrace}",
                    //        "Uncaught Thread Exception", System.Windows.MessageBoxButton.OK,
                    //        System.Windows.MessageBoxImage.Error);
                    //}, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);

                    }
                    catch (Exception ex)
                    {

                        throw;
                    }
                }, (arg) => true));
            }
}

        private void AddToViewImage(string imageFile, ObservableCollection<DirectoryWithFaces> directoriesWithFaces, 
            VoteAndDistance predict, int left, int top, int width, int height)
        {
            //this.Invoke(new Action(() =>
            Application.Current.Dispatcher.Invoke(() =>
            {
                //Image = new BitmapImage(croppedImage);
                try
                {
                    //BitmapImage src = new BitmapImage();
                    //src.BeginInit();
                    //src.UriSource = new Uri(imageFile, UriKind.Relative);
                    //src.CacheOption = BitmapCacheOption.OnLoad;
                    //src.EndInit();

                    //var src = new BitmapImage();
                    //using (var stream = File.OpenRead(imageFile))
                    //{
                    //    src.BeginInit();
                    //    src.CacheOption = BitmapCacheOption.OnLoad;
                    //    src.StreamSource = stream;
                    //    src.EndInit();
                    //    stream.Close();
                    //}

                    // Create an instance of WriteableBitmap from the original image.
                    // If you want to Skew or Rotate the cropped image, specify it as the second parameter.
                    // Just pass "null" if you want to use the original image directly.
                    //var writeableBitmap = new WriteableBitmap(src, null);

                    //// Crop and set the new WriteableBitmap as the image source
                    //CroppedImage.Source = CropImage(writeableBitmap, X, Y, WIDTH, HEIGHT);

                    System.Drawing.Bitmap croppedImage = ImageHelper.GetCroppedBitmap(_addBorder, 
                        imageFile, left, top, width, height);


                    //if (src.CanFreeze)
                    //    src.Freeze();

                    //if (left > src.PixelWidth)
                    //    throw new ArgumentException("left > src.Width");
                    //if (top > src.PixelHeight)
                    //    throw new ArgumentException("top > src.Height");
                    //int leftBorderAdded = left - addBorder >= 0 ? left - addBorder : left;
                    //int topBorderAdded = top - addBorder >= 0 ? top - addBorder : top;
                    //int widthBorderAdded = leftBorderAdded + width + addBorder * 2 > src.PixelWidth ?
                    //  (int)src.PixelWidth - leftBorderAdded : width + addBorder * 2;
                    //int heightBorderAdded = topBorderAdded + height + addBorder * 2 > src.PixelHeight ?
                    //    (int)src.PixelHeight - topBorderAdded : height + addBorder * 2;

                    //if (leftBorderAdded + widthBorderAdded > (int)src.PixelWidth)
                    //    throw new ArgumentException("leftBorderAdded + widthBorderAdded > (int)src.Width");
                    //if (topBorderAdded + heightBorderAdded > (int)src.PixelHeight)
                    //    throw new ArgumentException("topBorderAdded + heightBorderAdded > (int)src.Height");
                    //CroppedBitmap cropped = new CroppedBitmap(src, new Int32Rect(leftBorderAdded, topBorderAdded,
                    //    widthBorderAdded, heightBorderAdded));

                    //src = null;
                    //this.Image = cropped;

                    string directory = Path.GetDirectoryName(imageFile);
                    var dirWithFaces = directoriesWithFaces
                        .SingleOrDefault(dir => dir.Name == directory);
                    if (dirWithFaces == null)
                    {
                        dirWithFaces = new DirectoryWithFaces(directory);
                        directoriesWithFaces.Add(dirWithFaces);
                    }

                    //var image = dirWithFaces.Images.SingleOrDefault(im => im.Path == imageFile);
                    //if (image == null)
                    //{
                    //    image = new ImageInfo(imageFile);
                    //    dirWithFaces.Images.Add(image);
                    //}

                    ImageSource imageSource = ImageHelper.GetImageStream(croppedImage);
                    croppedImage.Dispose();
                    dirWithFaces.Faces.Add(new FaceInfo()
                    {
                        Image = imageSource,
                        Path = imageFile,
                        Predict = predict.Name,
                        Distance = predict.Distance,
                        ClassInfo = predict.ClassInfo
                    });
                }
                catch (Exception ex)
                {

                    throw;
                }

            });
        }

        private string _lastSavedClass;

        public Func<FaceInfo, BindableMenuItem[]> MenuGeneratorFaceInfo
        {
            get
            {
                return (faceInfo =>
                {
                    if (faceInfo != null)
                    {
                        ICommand saveAsPredictedCommand = new RelayCommand(arg =>
                        {
                            ImageHelper.SaveToClass(faceInfo.Predict, 
                                _configuration.TrainPath,
                                faceInfo.Path, faceInfo.ClassInfo.FaceLocation, _addBorder);
                            //BitmapEncoder encoder = new JpegBitmapEncoder();
                            //encoder.Frames.Add(BitmapFrame.Create((BitmapSource)faceInfo.Image));
                            //string fileName = Path.GetFileName(faceInfo.Path);
                            //string targetPath = Path.Combine(_configuration.TrainPath, faceInfo.Predict, fileName);
                            //if (File.Exists(targetPath))
                            //    throw new Exception($"File {targetPath} already exist!");
                            //using (var fileStream = new System.IO.FileStream(targetPath, System.IO.FileMode.Create))
                            //{
                            //    encoder.Save(fileStream);
                            //}
                            _lastSavedClass = faceInfo.Predict;
                        });

                        List<BindableMenuItem> menu = new List<BindableMenuItem>();
                        BindableMenuItem menuItem1 = new BindableMenuItem();
                        menuItem1.Name = $"Save as {faceInfo.Predict}";
                        menuItem1.Command = saveAsPredictedCommand;
                        menu.Add(menuItem1);

                        if (!String.IsNullOrEmpty(_lastSavedClass))
                        {
                            BindableMenuItem menuItem2 = new BindableMenuItem();
                            menuItem2.Name = $"Save as {_lastSavedClass}";
                            //menuItem2.Command = saveAsPredictedCommand;
                            menu.Add(menuItem2);
                        }

                        //ICommand saveAsCommand = new RelayCommand(arg =>
                        //{

                        //}, arg => _windowService != null);
                        //BindableMenuItem menuItem3 = new BindableMenuItem();
                        //menuItem3.Name = "Save as...";
                        ////menu[1].Command = saveAsCommand;
                        //menuItem3.Children =
                        //либо окно с ыфбром классов все же показывать
                        // menu.Add(menuItem2);

                        return menu.ToArray();
                    }
                    return null;
                });

            }
        }
    }
}

using DlibDotNet;
using DlibDotNet.Extensions;
using FaceRecognitionBusinessLogic;
using FaceRecognitionBusinessLogic.DataBase;
using FaceRecognitionBusinessLogic.KNN;
using FaceRecognitionBusinessLogic.ObjectModel;
using FaceRecognitionDotNet;
using FaceRecognitionWPF.Helper;
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
        public MainViewModel(View.WindowService windowService)
        {
            _windowService = windowService;

            TrainPath = @"d:\Борисов\WallpapersSort\FaceTrain";
            SearchPath = @"d:\Борисов\WallpapersSort\body";
            ModelsDirectory = @"d:\face_recognition_models";
            DistanceThreshold = 0.6;

            DirectoriesWithFaces = new ObservableCollection<DirectoryWithFaces>();

            _progress = new Progress<ProgressPartialResult>((result) =>
            {
                ProgressMaximum = result.Total;
                ProgressValue = result.Current;
                //ProgressValueTaskbar = (double)result.Current / (double)result.Total;
                //ProgressText = String.Format("{0} / {1}  {2}", result.Current, result.Total, result.Text);
            });
        }

        private IProgress<ProgressPartialResult> _progress;

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

        int _progressValue;
        public int ProgressValue
        {
            get => this._progressValue;
            set
            {
                this._progress = value;
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
                                            FaceEncodingInfo founded = db.GetFromDB(imageFile);
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


                                                            db.AddFaceInfo(imageFile, doubleInfo, faceBoundingBoxes.Single().Left, faceBoundingBoxes.Single().Right,
                                                                faceBoundingBoxes.Single().Top, faceBoundingBoxes.Single().Bottom);


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
                                                trainedInfo.Add(new ClassInfo(new DirectoryInfo(directory).Name,
                                                    founded.FingerAndLocations.Single().FingerPrint));
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

                                    SearchManager sm = new SearchManager(2, SearchPath, _progress, db);

                                var ext = new List<string> { ".jpg", ".jpeg", ".png" };
                                var searchFiles = System.IO.Directory.GetFiles(SearchPath, "*", SearchOption.AllDirectories)
                                .Where(s => ext.Contains(Path.GetExtension(s)));
                                    //string imageFile3 = @"d:\Борисов\WallpapersSort\body\-11072550_376068146.jpg";

                                    int progressCount = 0;
                                    ProgressMaximum = searchFiles.Count();
                                foreach (var imageFile in searchFiles)
                                    {
                                        progress.Report(progressCount);
                                        progressCount++;

                                        //load image
                                        //using (var ms = new MemoryStream(File.ReadAllBytes(imageFile3)))
                                        FaceEncodingInfo founded = db.GetFromDB(imageFile);
                                        if (founded == null)
                                        {
                                            FaceRecognitionDotNet.Image unknownImage;
                                            try
                                            {
                                                unknownImage = FaceRecognition.LoadImageFile(imageFile);
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
                                                Debug.WriteLine($"Read {imageFile}");
                                                //find face locations
                                                var locations = faceRecognition.FaceLocations(unknownImage);
                                                //# If no faces are found in the image, return an empty result.
                                                if (!locations.Any())
                                                {
                                                    Debug.WriteLine($"In {imageFile} not found faces");
                                                    db.AddFileWithoutFace(imageFile);
                                                    continue;
                                                }

                                                foreach (var location in locations)
                                                {
                                                    var encodings = faceRecognition.FaceEncodings(unknownImage, new Location[] { location });
                                                    if (encodings == null)
                                                        continue;

                                                    var encoding = encodings.Single();
                                                    //foreach (var encoding in encodings)
                                                    
                                                    var info = new SerializationInfo(typeof(double), formatterConverter);
                                                    encoding.GetObjectData(info, context);
                                                    encoding.Dispose();

                                                    double[] unknown = (double[])info.GetValue("_Encoding", typeof(double[]));
                                                    db.AddFaceInfo(imageFile, unknown, location.Left, location.Right,
                                                        location.Top, location.Bottom);

                                                    VoteAndDistance predict = MyKnn.Classify(unknown, trainedInfo, classes.ToArray(), 1);

                                                    if (predict.Distance < DistanceThreshold)
                                                    {
                                                        Debug.WriteLine($"Found {predict.Name} in {imageFile} with {predict.Distance} distance");
                                                        AddToViewImage(imageFile, DirectoriesWithFaces, predict, location.Left, location.Top,
                                                                        location.Right - location.Left, location.Bottom - location.Top);
                                                    }

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
                                        else
                                        {
                                            foreach (var fingerAndLocations in founded.FingerAndLocations)
                                            {
                                                VoteAndDistance predict = MyKnn.Classify(fingerAndLocations.FingerPrint,
                                                    trainedInfo, classes.ToArray(), 1);

                                                if (predict.Distance < DistanceThreshold)
                                                {
                                                    AddToViewImage(imageFile, DirectoriesWithFaces, predict,
                                                        fingerAndLocations.Left, fingerAndLocations.Top,
                                                        fingerAndLocations.Right - fingerAndLocations.Left,
                                                        fingerAndLocations.Bottom - fingerAndLocations.Top);

                                                }
                                            }
                                        }

                                    //int predicted = KNN.KNN.Classify(unknown, trainData, numClasses, k);
                                }
                                }
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
            const int addBorder = 30;
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

                    System.Drawing.Bitmap croppedImage;

                    using (var image = System.Drawing.Image.FromFile(imageFile))
                    using (var src = new System.Drawing.Bitmap(image))
                    {
                        if (left > src.Width)
                            throw new ArgumentException("left > src.Width");
                        if (top > src.Height)
                            throw new ArgumentException("top > src.Height");
                        int leftBorderAdded = left - addBorder >= 0 ? left - addBorder : left;
                        int topBorderAdded = top - addBorder >= 0 ? top - addBorder : top;
                        int widthBorderAdded = leftBorderAdded + width + addBorder * 2 > src.Width ?
                          (int)src.Width - leftBorderAdded : width + addBorder * 2;
                        int heightBorderAdded = topBorderAdded + height + addBorder * 2 > src.Height ?
                            (int)src.Height - topBorderAdded : height + addBorder * 2;

                        if (leftBorderAdded + widthBorderAdded > (int)src.Width)
                            throw new ArgumentException("leftBorderAdded + widthBorderAdded > (int)src.Width");
                        if (topBorderAdded + heightBorderAdded > (int)src.Height)
                            throw new ArgumentException("topBorderAdded + heightBorderAdded > (int)src.Height");
                        //CroppedBitmap cropped = new CroppedBitmap(src, new Int32Rect(leftBorderAdded, topBorderAdded,
                        //    widthBorderAdded, heightBorderAdded));
                        System.Drawing.Rectangle croppedRect = new System.Drawing.Rectangle(leftBorderAdded, topBorderAdded,
                            widthBorderAdded, heightBorderAdded);
                        croppedImage = new System.Drawing.Bitmap(widthBorderAdded, heightBorderAdded);
                        using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(croppedImage))
                        {
                            g.DrawImage(image, new System.Drawing.Rectangle(0, 0, widthBorderAdded, heightBorderAdded),
                                croppedRect,
                                System.Drawing.GraphicsUnit.Pixel);
                        }

                        //var resized = new Bitmap(size, size);
                        //var resizedPalette = resized.Palette;
                        //using (var graphics = Graphics.FromImage(resized))
                        //{
                        //    graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                        //    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        //    graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                        //    graphics.DrawImage(croppedImage, 0, 0, size, size);
                        //    using (var output = System.IO.File.Open(thumbnailPath, FileMode.Create))
                        //    {
                        //        resized.Save(output, System.Drawing.Imaging.ImageFormat.Jpeg);
                        //    }
                        //}
                    }

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
                        Predict = $"{predict.Name} {predict.Distance}"
                    });
                }
                catch (Exception ex)
                {

                    throw;
                }

            });
        }
    }
}

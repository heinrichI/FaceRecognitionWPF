using DlibDotNet;
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

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            DlibDotNet.Dlib.Encoding = System.Text.Encoding.GetEncoding(1251);
        }


        public void OnClosing(object sender, CancelEventArgs e)
        {
            _configuration.Save();
        }

        const float _percentageOfBorder = 0.3F;

        IEnumerable<string> _classes;

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

                            using (var db = TinyIoC.TinyIoCContainer.Current.Resolve<IDataBaseManager>(
                                new NamedParameterOverloads() { { "dataBaseName", "faces.litedb" } }))
                            {
                                List<ClassInfo> trainedInfo = new List<ClassInfo>();

                                TrainManager trainManager = new TrainManager(ref trainedInfo, out _classes,
                                    _configuration.ThreadCount, _configuration, db, _progress, _windowService);


                                SearchManager sm = new SearchManager(_configuration.ThreadCount, _configuration, _progress,
                                    db, trainedInfo, _classes,
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
            //Debug.WriteLine($"Добавление на форму {imageFile} left={left}, top={top}, width={width}, height={height}");
            //Debug.WriteLine($"{imageFile} predict left={predict.ClassInfo.FaceLocation.Left}, top={predict.ClassInfo.FaceLocation.Top}, right={predict.ClassInfo.FaceLocation.Right}, bottom={predict.ClassInfo.FaceLocation.Bottom}");
            //if (left != predict.ClassInfo.FaceLocation.Left)
            //    throw new ArgumentException($"Неправильно переданы координаты для {imageFile} {left} != {predict.ClassInfo.FaceLocation.Left}");
            //this.Invoke(new Action(() =>
            Application.Current.Dispatcher.Invoke(() =>
            {
                //Image = new BitmapImage(croppedImage);
                try
                {
                    System.Drawing.Bitmap croppedImage = ImageHelper.GetCroppedBitmap(_percentageOfBorder,
                        imageFile, left, top, width, height);

                    string directory = Path.GetDirectoryName(imageFile);
                    var dirWithFaces = directoriesWithFaces
                        .SingleOrDefault(dir => dir.Name == directory);
                    if (dirWithFaces == null)
                    {
                        dirWithFaces = new DirectoryWithFaces(directory);
                        directoriesWithFaces.Add(dirWithFaces);
                    }

                    ImageSource imageSource = ImageHelper.GetImageStream(croppedImage);
                    croppedImage.Dispose();
                    dirWithFaces.Faces.Add(new FaceInfo()
                    {
                        Image = imageSource,
                        Path = imageFile,
                        Predict = predict.Name,
                        Distance = predict.Distance,
                        Left = left,
                        Top = top,
                        Width = width,
                        Height = height,
                        SortedInfos = predict.SortedInfos,
                        TestData = predict.TestData
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
                            //Debug.WriteLine($"Сохранения изображения {faceInfo.Path} c координатами left={faceInfo.ClassInfo.FaceLocation.Left}, top={faceInfo.ClassInfo.FaceLocation.Top}, right={faceInfo.ClassInfo.FaceLocation.Right}, bottom={faceInfo.ClassInfo.FaceLocation.Bottom}");
                            //FaceViewModel vm = new FaceViewModel(faceInfo.ClassInfo.FaceLocation, faceInfo.Path);
                            //_windowService.ShowDialogWindow<FaceWindow>(vm);

                            ImageHelper.SaveToClass(faceInfo.Predict,
                                _configuration.TrainPath,
                                faceInfo.Path, _percentageOfBorder, faceInfo.Left, faceInfo.Top, faceInfo.Width, faceInfo.Height);
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

                        if (!String.IsNullOrEmpty(_lastSavedClass) && faceInfo.Predict != _lastSavedClass)
                        {
                            BindableMenuItem menuItem2 = new BindableMenuItem();
                            menuItem2.Name = $"Save as {_lastSavedClass}";
                            //menuItem2.Command = saveAsPredictedCommand;
                            menu.Add(menuItem2);
                        }

                        ICommand showInfoCommand = new RelayCommand(arg =>
                        {
                            InfoViewModel vm = new InfoViewModel(faceInfo);
                            _windowService.ShowDialogWindow<InfoWindow>(vm);
                        });
                        BindableMenuItem menuItemShowInfo = new BindableMenuItem();
                        menuItemShowInfo.Name = "Show info";
                        menuItemShowInfo.Command = showInfoCommand;
                        menu.Add(menuItemShowInfo);

                        ICommand saveAsCommand = new RelayCommand(arg =>
                        {
                            ClassSelecterViewModel vm = new ClassSelecterViewModel(_classes, _configuration);
                            _windowService.ShowDialogWindow<ClassSelecterWindow>(vm);
                        }, arg => _windowService != null);
                        BindableMenuItem menuItemSaveAs = new BindableMenuItem();
                        menuItemSaveAs.Name = "Save as...";
                        menuItemSaveAs.Command = saveAsCommand;
                        menu.Add(menuItemSaveAs);

                        return menu.ToArray();
                    }
                    return null;
                });

            }
        }


        private RelayCommand _checkDataBaseCommand;
        public RelayCommand CheckDataBaseCommand
        {
            get
            {
                return _checkDataBaseCommand ?? (_checkDataBaseCommand = new RelayCommand((arg) =>
                {
                    //await 
                    Task.Run(() =>
                    {
                        using (var db = TinyIoC.TinyIoCContainer.Current.Resolve<IDataBaseManager>(
    new NamedParameterOverloads() { { "dataBaseName", "faces.litedb" } }))
                        {
                            CheckManager checkManager = new CheckManager(db, _configuration, _progress);
                            MessageBox.Show("Checked!");
                        }
                    });
                }, (arg) => true));
            }
        }


        private RelayCommand _showAnotherClassAndDistanceMoreThan;
        public RelayCommand ShowAnotherClassAndDistanceMoreThan
        {
            get
            {
                return _showAnotherClassAndDistanceMoreThan ?? (_showAnotherClassAndDistanceMoreThan = new RelayCommand(async (arg) =>
                {
                }, (arg) => true));
            }
        }
    }
}

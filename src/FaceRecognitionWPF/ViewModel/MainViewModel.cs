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
using System.Threading;
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
                if (result.Total == result.Current)
                    ProgressVisible = Visibility.Collapsed;
                else
                {
                    ProgressVisible = Visibility.Visible;
                    ProgressMaximum = result.Total;
                    ProgressValue = result.Current;
                    //ProgressValueTaskbar = (double)result.Current / (double)result.Total;
                    ProgressText = String.Format("{0} / {1}  {2}", result.Current, result.Total, result.Text);
                }
            });

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            DlibDotNet.Dlib.Encoding = System.Text.Encoding.GetEncoding(1251);
        }


        public void OnClosing(object sender, CancelEventArgs e)
        {
            _configuration.Save();
        }

        const float _percentageOfBorder = 0.3F;

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
            get => _progressText;
            set
            {
                _progressText = value;
                this.OnPropertyChanged();
            }
        }

        Visibility _progressVisible;
        public Visibility ProgressVisible
        {
            get => _progressVisible;
            set
            {
                if (_progressVisible != value)
                {
                    _progressVisible = value;
                    this.OnPropertyChanged();
                }
            }
        }

        List<ClassInfo> _trainedInfo = new List<ClassInfo>();

        IEnumerable<string> _classes;
        public IEnumerable<string> Classes
        {
            get => this._classes;
            set
            {
                this._classes = value;
                this.OnPropertyChanged();
            }
        }

        string _checkClass;
        public string CheckClass
        {
            get => this._checkClass;
            set
            {
                this._checkClass = value;
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
                                if (_trainedInfo == null || !_trainedInfo.Any())
                                {
                                    TrainManager trainManager = new TrainManager(ref _trainedInfo,
                                        _configuration, db, _progress, _windowService);
                                    Classes = trainManager.Train(_configuration.ThreadCount);
                                }

                                SearchManager sm = new SearchManager(_configuration.ThreadCount, _configuration, _progress,
                                    db, _trainedInfo, _classes,
                                    DirectoriesWithFaces, AddToViewImage, CheckClass);
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
            VoteAndDistance predict, int left, int top, int width, int height, int locationsCount)
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
                        TestData = predict.TestData,
                        LocationsCount = locationsCount
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
                        List<BindableMenuItem> menu = new List<BindableMenuItem>();

                        if (faceInfo.LocationsCount > 1)
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
                            BindableMenuItem menuItem1 = new BindableMenuItem();
                            menuItem1.Name = $"Save as {faceInfo.Predict}";
                            menuItem1.Command = saveAsPredictedCommand;
                            menu.Add(menuItem1);


                            if (!String.IsNullOrEmpty(_lastSavedClass) && faceInfo.Predict != _lastSavedClass)
                            {
                                ICommand saveAsLastSavedCommand = new RelayCommand(arg =>
                                {
                                    ImageHelper.SaveToClass(_lastSavedClass,
                                            _configuration.TrainPath,
                                            faceInfo.Path, _percentageOfBorder, faceInfo.Left, faceInfo.Top, faceInfo.Width, faceInfo.Height);
                                });
                                BindableMenuItem menuItemSaveLast = new BindableMenuItem();
                                menuItemSaveLast.Name = $"Save as {_lastSavedClass}";
                                menuItemSaveLast.Command = saveAsLastSavedCommand;
                                menu.Add(menuItemSaveLast);
                            }

                            ICommand saveAsCommand = new RelayCommand(arg =>
                            {
                                ClassSelecterViewModel vm = new ClassSelecterViewModel(_classes);
                                var result = _windowService.ShowDialogWindow<ClassSelecterWindow>(vm);
                                if (result.HasValue && result.Value)
                                {
                                    ImageHelper.SaveToClass(vm.SelectedClass,
                                       _configuration.TrainPath,
                                       faceInfo.Path, _percentageOfBorder, faceInfo.Left, faceInfo.Top, faceInfo.Width, faceInfo.Height);
                                    _lastSavedClass = vm.SelectedClass;
                                }
                            }, arg => _windowService != null);
                            BindableMenuItem menuItemSaveAs = new BindableMenuItem();
                            menuItemSaveAs.Name = "Save as...";
                            menuItemSaveAs.Command = saveAsCommand;
                            menu.Add(menuItemSaveAs);


                            if (!String.IsNullOrEmpty(_lastSavedClass) && faceInfo.Predict != _lastSavedClass)
                            {
                                BindableMenuItem menuItem2 = new BindableMenuItem();
                                menuItem2.Name = $"Save as {_lastSavedClass}";
                                menu.Add(menuItem2);
                            }
                        }
                        else
                        {
                            ICommand copyToPredictCommand = new RelayCommand(arg =>
                            {
                                string targetPath = ImageHelper.GetTargetPath(faceInfo.Predict,
                                    _configuration.TrainPath, faceInfo.Path);
                                File.Copy(faceInfo.Path, targetPath);
                                _lastSavedClass = faceInfo.Predict;
                            }, arg => _windowService != null);
                            BindableMenuItem menuItemCopyToPredict = new BindableMenuItem();
                            menuItemCopyToPredict.Name = $"Copy to {faceInfo.Predict}";
                            menuItemCopyToPredict.Command = copyToPredictCommand;
                            menu.Add(menuItemCopyToPredict);

                            if (!String.IsNullOrEmpty(_lastSavedClass) && faceInfo.Predict != _lastSavedClass)
                            {
                                ICommand copyLastSavedCommand = new RelayCommand(arg =>
                                {
                                    string targetPath = ImageHelper.GetTargetPath(_lastSavedClass,
                                                                     _configuration.TrainPath, faceInfo.Path);
                                    File.Copy(faceInfo.Path, targetPath);
                                });
                                BindableMenuItem menuItemCopyLast = new BindableMenuItem();
                                menuItemCopyLast.Name = $"Copy to {_lastSavedClass}";
                                menuItemCopyLast.Command = copyLastSavedCommand;
                                menu.Add(menuItemCopyLast);
                            }
                        }


                        ICommand copyToCommand = new RelayCommand(arg =>
                        {
                            ClassSelecterViewModel vm = new ClassSelecterViewModel(_classes);
                            var result = _windowService.ShowDialogWindow<ClassSelecterWindow>(vm);
                            if (result.HasValue && result.Value)
                            {
                                string targetPath = ImageHelper.GetTargetPath(vm.SelectedClass,
                                   _configuration.TrainPath, faceInfo.Path);
                                File.Copy(faceInfo.Path, targetPath);
                                _lastSavedClass = vm.SelectedClass;
                            }
                        }, arg => _windowService != null);
                        BindableMenuItem menuItemCopyTo = new BindableMenuItem();
                        menuItemCopyTo.Name = "Copy to...";
                        menuItemCopyTo.Command = copyToCommand;
                        menu.Add(menuItemCopyTo);

                        ICommand showInfoCommand = new RelayCommand(arg =>
                        {
                            InfoViewModel vm = new InfoViewModel(faceInfo);
                            var result = _windowService.ShowDialogWindow<InfoWindow>(vm);
                        });
                        BindableMenuItem menuItemShowInfo = new BindableMenuItem();
                        menuItemShowInfo.Name = "Show info";
                        menuItemShowInfo.Command = showInfoCommand;
                        menu.Add(menuItemShowInfo);

                        return menu.ToArray();
                    }
                    return null;
                });

            }
        }


        private RelayCommand _showAnotherClassAndDistanceMoreThanCommand;
        public RelayCommand ShowAnotherClassAndDistanceMoreThanCommand
        {
            get
            {
                return _showAnotherClassAndDistanceMoreThanCommand ?? (_showAnotherClassAndDistanceMoreThanCommand = new RelayCommand((arg) =>
                {
                    Task.Run(() =>
                    {
                        using (var db = TinyIoC.TinyIoCContainer.Current.Resolve<IDataBaseManager>(
                            new NamedParameterOverloads() { { "dataBaseName", "faces.litedb" } }))
                        {
                            if (_trainedInfo == null || !_trainedInfo.Any())
                            {
                                TrainManager trainManager = new TrainManager(ref _trainedInfo,
                                    _configuration, db, _progress, _windowService);
                                Classes = trainManager.Train(_configuration.ThreadCount);
                            }

                            SearchManager sm = new SearchManager(_configuration.ThreadCount, _configuration, _progress,
                                db, _trainedInfo, _classes,
                                DirectoriesWithFaces, AddToViewImage, CheckClass);
                        }
                        MessageBox.Show("Done!");
                    });
                }, (arg) => !String.IsNullOrEmpty(CheckClass)));
            }
        }

        private RelayCommand _trainCommand;
        public RelayCommand TrainCommand
        {
            get
            {
                return _trainCommand ?? (_trainCommand = new RelayCommand((arg) =>
                {
                    TaskScheduler syncContextScheduler;
                    if (SynchronizationContext.Current != null)
                    {
                        syncContextScheduler = TaskScheduler.FromCurrentSynchronizationContext();
                    }
                    else
                    {
                        // If there is no SyncContext for this thread (e.g. we are in a unit test
                        // or console scenario instead of running in an app), then just use the
                        // default scheduler because there is no UI thread to sync with.
                        syncContextScheduler = TaskScheduler.Current;
                    }

                    Task.Run(() =>
                    {
                        using (var db = TinyIoC.TinyIoCContainer.Current.Resolve<IDataBaseManager>(
                            new NamedParameterOverloads() { { "dataBaseName", "faces.litedb" } }))
                        {
                            if (_trainedInfo == null || !_trainedInfo.Any())
                            {
                                TrainManager trainManager = new TrainManager(ref _trainedInfo,
                                    _configuration, db, _progress, _windowService);
                                Classes = trainManager.Train(_configuration.ThreadCount);
                            }
                        }
                    })
                    .ContinueWith(t =>
                    {
                        _trainCommand.RaiseCanExecuteChanged();
                    }, syncContextScheduler);


                }, (arg) => _trainedInfo == null || !_trainedInfo.Any()));
            }
        }

        private RelayCommand _aboutCommand;
        public RelayCommand AboutCommand
        {
            get
            {
                return _aboutCommand ?? (_aboutCommand = new RelayCommand((arg) =>
                {
                    _windowService.ShowAboutWindow();
                }));
            }
        }


        private RelayCommand _checkDataBaseCommand;
        public RelayCommand CheckDataBaseCommand
        {
            get
            {
                return _checkDataBaseCommand ?? (_checkDataBaseCommand = new RelayCommand((arg) =>
                {
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

        private RelayCommand _convertToLowerCaseCommand;
        public RelayCommand ConvertToLowerCaseCommand
        {
            get
            {
                return _convertToLowerCaseCommand ?? (_convertToLowerCaseCommand = new RelayCommand((arg) =>
                {
                    Task.Run(() =>
                    {
                        using (var db = TinyIoC.TinyIoCContainer.Current.Resolve<IDataBaseManager>(
    new NamedParameterOverloads() { { "dataBaseName", "faces.litedb" } }))
                        {
                            ConvertToLowerCaseManager checkManager = new ConvertToLowerCaseManager(db, _progress);
                            var message = checkManager.Convert();
                            MessageBox.Show($"Converted done, {message}");
                        }
                    });
                }));
            }
        }

        private RelayCommand _removeRecordForUnexistFilesCommand;
        public RelayCommand RemoveRecordForUnexistFilesCommand
        {
            get
            {
                return _removeRecordForUnexistFilesCommand ?? (_removeRecordForUnexistFilesCommand = new RelayCommand((arg) =>
                {
                    Task.Run(() =>
                    {
                        using (var db = TinyIoC.TinyIoCContainer.Current.Resolve<IDataBaseManager>(
    new NamedParameterOverloads() { { "dataBaseName", "faces.litedb" } }))
                        {
                            RemoveRecordForUnexistFilesManager checkManager = 
                                new RemoveRecordForUnexistFilesManager(db, _progress);
                            var message = checkManager.Remove();
                            MessageBox.Show($"Remove done, {message}");
                        }
                    });
                }));
            }
        }
        

    }
}

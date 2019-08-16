using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FaceRecognitionBusinessLogic;
using FaceRecognitionBusinessLogic.ObjectModel;
using FaceRecognitionDotNet;

namespace FaceRecognitionWPF.ViewModel
{
    class FaceViewModel : CloseableViewModel
    {
        string _imagePath;
        public FaceViewModel(IEnumerable<Location> faceBoundingBoxes, string imagePath)
        {
            _imagePath = imagePath;

            Title = imagePath;
            Application.Current.Dispatcher.Invoke(() =>
            {
                BitmapImage src = new BitmapImage();
                src.BeginInit();
                src.UriSource = new Uri(imagePath, UriKind.Absolute);
                src.CacheOption = BitmapCacheOption.OnLoad;
                src.EndInit();

                DrawingVisual dv = new DrawingVisual();
                using (DrawingContext dc = dv.RenderOpen())
                {
                    dc.DrawImage(src, new Rect(0, 0, src.PixelWidth, src.PixelHeight));
                    foreach (var faceBoundingBox in faceBoundingBoxes)
                    {
                        dc.DrawRectangle(System.Windows.Media.Brushes.Green, null, new Rect(faceBoundingBox.Left,
                            faceBoundingBox.Top, faceBoundingBox.Right - faceBoundingBox.Left, 
                            faceBoundingBox.Bottom - faceBoundingBox.Top));
                    }
                    
                }

                RenderTargetBitmap rtb = new RenderTargetBitmap(src.PixelWidth, src.PixelHeight, 96, 96, 
                    PixelFormats.Pbgra32);
                rtb.Render(dv);

                Image = rtb;
            });
        }

        public FaceViewModel(FaceLocation faceLocation, string imagePath)
        {
            _imagePath = imagePath;

            Title = imagePath;
            Application.Current.Dispatcher.Invoke(() =>
            {
                BitmapImage src = new BitmapImage();
                src.BeginInit();
                src.UriSource = new Uri(imagePath, UriKind.Absolute);
                src.CacheOption = BitmapCacheOption.OnLoad;
                src.EndInit();

                DrawingVisual dv = new DrawingVisual();
                using (DrawingContext dc = dv.RenderOpen())
                {
                    dc.DrawImage(src, new Rect(0, 0, src.PixelWidth, src.PixelHeight));
                    dc.DrawRectangle(System.Windows.Media.Brushes.Green, null, 
                        new Rect(faceLocation.Left,
                        faceLocation.Top, faceLocation.Right - faceLocation.Left,
                        faceLocation.Bottom - faceLocation.Top));
                }

                RenderTargetBitmap rtb = new RenderTargetBitmap(src.PixelWidth, src.PixelHeight, 96, 96,
                    PixelFormats.Pbgra32);
                rtb.Render(dv);

                Image = rtb;
            });
        }

        public FaceViewModel(string imagePath)
        {
            _imagePath = imagePath;

            Title = $"Not found face in {imagePath}";
            Application.Current.Dispatcher.Invoke(() =>
            {
                BitmapImage src = new BitmapImage();
                src.BeginInit();
                src.UriSource = new Uri(imagePath, UriKind.Absolute);
                src.CacheOption = BitmapCacheOption.OnLoad;
                src.EndInit();

                Image = src;
            });
        }

        public void OnClosing(object sender, CancelEventArgs e)
        {
        }


        ImageSource _image;
        public ImageSource Image
        {
            get => this._image;
            set
            {
                this._image = value;
                this.OnPropertyChanged();
            }
        }

        string _title;
        public string Title
        {
            get => this._title;
            set
            {
                this._title = value;
                this.OnPropertyChanged();
            }
        }

        private RelayCommand _deleteFileCommand;
        public RelayCommand DeleteFileCommand
        {
            get
            {
                return _deleteFileCommand ?? (_deleteFileCommand = new RelayCommand((arg) =>
                {
                    if (MessageBox.Show($"Are you shure want to delete {_imagePath}?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        System.IO.File.Delete(_imagePath);
                    }
                    base.RaiseClosingRequest(true);
                }, (arg) => !String.IsNullOrEmpty(_imagePath)));
            }
        }
    }
}

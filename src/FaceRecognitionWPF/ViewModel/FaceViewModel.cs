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

namespace FaceRecognitionWPF.ViewModel
{
    // Создаётся только UI-адаптером (WindowService) уже в UI-потоке.
    class FaceViewModel : CloseableViewModel
    {
        string _imagePath;
        public FaceViewModel(IEnumerable<FaceLocation> faceLocations, string imagePath)
        {
            _imagePath = imagePath;

            var boxes = (faceLocations ?? Enumerable.Empty<FaceLocation>()).ToList();
            Title = boxes.Any() ? imagePath : $"Not found face in {imagePath}";

            BitmapImage src = new BitmapImage();
            src.BeginInit();
            src.UriSource = new Uri(imagePath, UriKind.Absolute);
            src.CacheOption = BitmapCacheOption.OnLoad;
            src.EndInit();

            if (boxes.Any())
            {
                DrawingVisual dv = new DrawingVisual();
                using (DrawingContext dc = dv.RenderOpen())
                {
                    dc.DrawImage(src, new Rect(0, 0, src.PixelWidth, src.PixelHeight));
                    foreach (var box in boxes)
                    {
                        dc.DrawRectangle(Brushes.Green, null, new Rect(box.Left,
                            box.Top, box.Right - box.Left,
                            box.Bottom - box.Top));
                    }
                }

                RenderTargetBitmap rtb = new RenderTargetBitmap(src.PixelWidth, src.PixelHeight, 96, 96,
                    PixelFormats.Pbgra32);
                rtb.Render(dv);

                Image = rtb;
            }
            else
            {
                Image = src;
            }
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
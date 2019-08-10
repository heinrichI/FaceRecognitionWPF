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
using FaceRecognitionDotNet;

namespace FaceRecognitionWPF.ViewModel
{
    class FaceViewModel : BasePropertyChanged, IClosingViewModel
    {

        public FaceViewModel(IEnumerable<Location> faceBoundingBoxes, string imageFile)
        {
            Title = imageFile;
            Application.Current.Dispatcher.Invoke(() =>
            {
                BitmapImage src = new BitmapImage();
                src.BeginInit();
                src.UriSource = new Uri(imageFile, UriKind.Absolute);
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

        public FaceViewModel(string imageFile)
        {
            Title = $"Not found face in {imageFile}";
            Application.Current.Dispatcher.Invoke(() =>
            {
                BitmapImage src = new BitmapImage();
                src.BeginInit();
                src.UriSource = new Uri(imageFile, UriKind.Absolute);
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
        private string imageFile;

        public string Title
        {
            get => this._title;
            set
            {
                this._title = value;
                this.OnPropertyChanged();
            }
        }
    }
}

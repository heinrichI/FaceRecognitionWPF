using FaceRecognitionBusinessLogic;
using FaceRecognitionBusinessLogic.ObjectModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FaceRecognitionWPF.ViewModel
{
    public class InfoViewModel : CloseableViewModel
    {
        public InfoViewModel(FaceInfo faceInfo)
        {
            TrainData = faceInfo.SortedInfos.First().Data;
            TestData = faceInfo.TestData;

            TrainPath = faceInfo.SortedInfos.First().ImagePath;
            TestPath = faceInfo.Path;

            BitmapImage trainBitmap = new BitmapImage();
            trainBitmap.BeginInit();
            trainBitmap.UriSource = new Uri(TrainPath, UriKind.Relative);
            trainBitmap.CacheOption = BitmapCacheOption.OnLoad;
            trainBitmap.EndInit();
            if (trainBitmap.CanFreeze)
                trainBitmap.Freeze();
            TrainImage = trainBitmap;

            BitmapImage testBitmap = new BitmapImage();
            testBitmap.BeginInit();
            testBitmap.UriSource = new Uri(TestPath, UriKind.Relative);
            testBitmap.CacheOption = BitmapCacheOption.OnLoad;
            testBitmap.EndInit();
            if (testBitmap.CanFreeze)
                testBitmap.Freeze();
            TestImage = testBitmap;
        }

        public void OnClosing(object sender, CancelEventArgs e)
        {
        }

        double[] _trainData;
        public double[] TrainData
        {
            get => _trainData;
            set
            {
                _trainData = value;
                OnPropertyChanged();
            }
        }


        double[] _testData;
        public double[] TestData
        {
            get => _testData;
            set
            {
                _testData = value;
                OnPropertyChanged();
            }
        }

        string _trainPath;
        public string TrainPath
        {
            get => _trainPath;
            set
            {
                _trainPath = value;
                OnPropertyChanged();
            }
        }

        string _testPath;
        public string TestPath
        {
            get => _testPath;
            set
            {
                _testPath = value;
                OnPropertyChanged();
            }
        }

        ImageSource _trainImage;
        public ImageSource TrainImage
        {
            get => _trainImage;
            set
            {
                _trainImage = value;
                OnPropertyChanged();
            }
        }

        ImageSource _testImage;
        public ImageSource TestImage
        {
            get => _testImage;
            set
            {
                _testImage = value;
                OnPropertyChanged();
            }
        }
    }
}

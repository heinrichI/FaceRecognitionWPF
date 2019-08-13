using FaceRecognitionBusinessLogic;
using FaceRecognitionBusinessLogic.ObjectModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionWPF.ViewModel
{
    public class InfoViewModel : BasePropertyChanged, IClosingViewModel
    {


        public InfoViewModel(FaceInfo faceInfo)
        {
            TrainData = faceInfo.SortedInfos.First().Data;
            TestData = faceInfo.TestData;

            TrainPath = faceInfo.SortedInfos.First().ImagePath;
        }

        public void OnClosing(object sender, CancelEventArgs e)
        {
        }

        double[] _trainData;
        public double[] TrainData
        {
            get => this._trainData;
            set
            {
                this._trainData = value;
                this.OnPropertyChanged();
            }
        }

        string _trainPath;
        public string TrainPath
        {
            get => this._trainPath;
            set
            {
                this._trainPath = value;
                this.OnPropertyChanged();
            }
        }

        double[] _testData;
        public double[] TestData
        {
            get => this._testData;
            set
            {
                this._testData = value;
                this.OnPropertyChanged();
            }
        }
    }
}

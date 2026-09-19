using FaceRecognitionBusinessLogic.KNN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FaceRecognitionBusinessLogic.ObjectModel
{
    public class FaceInfo : BasePropertyChanged
    {
        // Ядро не зависит от WPF: в WPF-слое сюда кладут ImageSource (обёртка-DTO).
        object _image;
        [XmlIgnoreAttribute]
        public object Image
        {
            get => this._image;
            set
            {
                this._image = value;
                this.OnPropertyChanged();
                //this.RaisePropertyChangedEvent("Image");
            }
        }

        string _path;
        public string Path
        {
            get => this._path;
            set
            {
                this._path = value;
                this.OnPropertyChanged();
            }
        }

        string _predict;
        public string Predict
        {
            get => this._predict;
            set
            {
                this._predict = value;
                this.OnPropertyChanged();
            }
        }

        double _distance;
        public double Distance
        {
            get => this._distance;
            set
            {
                this._distance = value;
                this.OnPropertyChanged();
            }
        }

        public int Left { get; set; }

        public int Top { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public List<ClassInfo> SortedInfos { get; set; }

        public double[] TestData { get; set; }

        public int LocationsCount { get; set; }
    }
}

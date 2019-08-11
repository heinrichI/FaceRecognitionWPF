using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Serialization;

namespace FaceRecognitionBusinessLogic.ObjectModel
{
    public class FaceInfo : BasePropertyChanged
    {
        [NonSerialized]
        [XmlIgnoreAttribute]
        ImageSource _image;
        [XmlIgnoreAttribute]
        public ImageSource Image
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
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionBusinessLogic.ObjectModel
{
    public class DirectoryWithFaces : BasePropertyChanged
    {
        public DirectoryWithFaces(string name)
        {
            Name = name;
            Faces = new ObservableCollection<FaceInfo>();
        }

        ObservableCollection<FaceInfo> _faces;
        public ObservableCollection<FaceInfo> Faces
        {
            get => this._faces;
            set
            {
                this._faces = value;
                this.OnPropertyChanged();
            }
        }

        //ObservableCollection<ImageInfo> _images;
        //public ObservableCollection<ImageInfo> Images
        //{
        //    get => this._images;
        //    set
        //    {
        //        this._images = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        public string Name { get;  }
    }
}

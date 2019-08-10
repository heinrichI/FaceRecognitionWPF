using FaceRecognitionBusinessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionDataBase
{
    public class Configuration : BasePropertyChanged, IConfiguration
    {
        [NonSerialized]
        private const string _fileName = @"configuration.xml";

        string _trainPath;
        public string TrainPath
        {
            get => this._trainPath;
            set
            {
                this._trainPath = value;
                this.RaisePropertyChangedEvent("TrainPath");
            }
        }

        string _searchPath;
        public string SearchPath
        {
            get => this._searchPath;
            set
            {
                this._searchPath = value;
                this.RaisePropertyChangedEvent("SearchPath");
            }
        }

        string _modelsDirectory;
        public string ModelsDirectory
        {
            get => this._modelsDirectory;
            set
            {
                this._modelsDirectory = value;
                this.RaisePropertyChangedEvent("ModelsDirectory");
            }
        }

        double _distanceThreshold = 0.6;
        public double DistanceThreshold
        {
            get => this._distanceThreshold;
            set
            {
                this._distanceThreshold = value;
                this.RaisePropertyChangedEvent("DistanceThreshold");
            }
        }

        public static Configuration Load()
        {
            Configuration model = SerializeHelper<Configuration>.Load(_fileName);
            if (model == null)
            {
                model = new Configuration();
            }

            return model;
        }


        public void Save()
        {
            SerializeHelper<Configuration>.Save(this, _fileName);
        }
    }
}

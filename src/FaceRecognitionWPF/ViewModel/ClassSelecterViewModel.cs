using FaceRecognitionBusinessLogic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionWPF.ViewModel
{
    public class ClassSelecterViewModel : CloseableViewModel
    {
        public ClassSelecterViewModel(IEnumerable<string> classes)
        {
            Сlasses = classes;
        }

        IEnumerable<string> _сlasses;
        public IEnumerable<string> Сlasses
        {
            get => this._сlasses;
            set
            {
                this._сlasses = value;
                this.OnPropertyChanged();
            }
        }

        string _selectedClass;
        public string SelectedClass
        {
            get => _selectedClass;
            set
            {
                this._selectedClass = value;
                this.OnPropertyChanged();
            }
        }

        private RelayCommand _selectCommand;
        public RelayCommand SelectCommand
        {
            get
            {
                return _selectCommand ?? (_selectCommand = new RelayCommand((arg) =>
                {
                    base.RaiseClosingRequest(true);
                }, (arg) => !String.IsNullOrEmpty(SelectedClass)));
            }
        }
    }
}

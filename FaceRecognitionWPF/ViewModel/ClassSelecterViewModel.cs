using FaceRecognitionBusinessLogic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionWPF.ViewModel
{
    public class ClassSelecterViewModel : BasePropertyChanged, IClosingViewModel
    {
        private IEnumerable<string> _classes;
        private IConfiguration _configuration;

        public ClassSelecterViewModel(IEnumerable<string> classes, IConfiguration configuration)
        {
            _classes = classes;
            _configuration = configuration;
        }

        public void OnClosing(object sender, CancelEventArgs e)
        {
        }
    }
}
